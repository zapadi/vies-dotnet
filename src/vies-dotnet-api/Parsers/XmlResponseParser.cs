/*
   Copyright 2017-2024 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Padi.Vies.Errors;

namespace Padi.Vies.Parsers;

public sealed class XmlResponseParser : IResponseParserAsync
{
    private static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        CheckCharacters = false,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        CloseInput = true,
        Async = true,
    };

    public ViesCheckVatResponse Parse(Stream response)
    {
        using (var xmlReader = XmlReader.Create(response, XmlReaderSettings))
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (ViesKeys.Fault.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    (_, string error) = ReadError(xmlReader);
                    throw new ViesServiceException(error);
                }

                if (!ViesKeys.CheckVatResponse.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return ReadResponse(xmlReader);
            }
        }

        throw new ViesDeserializationException($"Could not deserialize response: {response}");
    }

    public async Task<ViesCheckVatResponse> ParseAsync(Stream response)
    {
        using (var xmlReader = XmlReader.Create(response, XmlReaderSettings))
        {
            while (await xmlReader.ReadAsync().ConfigureAwait(false))
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (ViesKeys.Fault.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    (_, string error) = await ReadErrorAsync(xmlReader).ConfigureAwait(false);
                    throw new ViesServiceException(error);
                }

                if (!ViesKeys.CheckVatResponse.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return await ReadResponseAsync(xmlReader).ConfigureAwait(false);
            }
        }

        throw new ViesDeserializationException($"Could not deserialize stream response");
    }

    private static ViesCheckVatResponse ReadResponse(XmlReader xmlReader)
    {
        var viesCheckVatResponse = new ViesCheckVatResponse();

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            switch (xmlReader.LocalName)
            {
                case ViesKeys.CountryCode:
                    viesCheckVatResponse.CountryCode = GetValue<string>(xmlReader);
                    break;
                case ViesKeys.VatNumber:
                    viesCheckVatResponse.VatNumber = GetValue<string>(xmlReader);
                    break;
                case ViesKeys.RequestDate:
                    var dt = GetValue<DateTime>(xmlReader, CultureInfo.InvariantCulture);
                    viesCheckVatResponse.RequestDate =new DateTimeOffset(dt);
                    break;
                case ViesKeys.Valid:
                    viesCheckVatResponse.IsValid = GetValue<bool>(xmlReader);
                    break;
                case ViesKeys.Name:
                    viesCheckVatResponse.Name = GetValue<string>(xmlReader);
                    break;
                case ViesKeys.Address:
                    viesCheckVatResponse.Address = GetValue<string>(xmlReader);
                    break;
                default:
                    xmlReader.Read();
                    break;
            }
        }

        return viesCheckVatResponse;
    }

    private static async Task<ViesCheckVatResponse> ReadResponseAsync(XmlReader xmlReader)
    {
        var viesCheckVatResponse = new ViesCheckVatResponse();

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            switch (xmlReader.LocalName.ToUpperInvariant())
            {
                case ViesKeys.CountryCode:
                    viesCheckVatResponse.CountryCode = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                    break;
                case ViesKeys.VatNumber:
                    viesCheckVatResponse.VatNumber = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                    break;
                case ViesKeys.RequestDate:
                    var dt = await GetValueAsync<DateTime>(xmlReader, CultureInfo.InvariantCulture).ConfigureAwait(false);
                    viesCheckVatResponse.RequestDate = new DateTimeOffset(dt);
                    break;
                case ViesKeys.Valid:
                    viesCheckVatResponse.IsValid = await GetValueAsync<bool>(xmlReader).ConfigureAwait(false);
                    break;
                case ViesKeys.Name:
                    viesCheckVatResponse.Name = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                    break;
                case ViesKeys.Address:
                    viesCheckVatResponse.Address = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                    break;
                default:
                    await xmlReader.ReadAsync().ConfigureAwait(false);
                    break;
            }
        }

        return viesCheckVatResponse;
    }

    private static (string code, string error) ReadError(XmlReader xmlReader)
    {
        string faultCode = null, faultMessage = null;

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            switch (xmlReader.LocalName.ToUpperInvariant())
            {
                case ViesKeys.FaultCode:
                    faultCode = GetValue<string>(xmlReader);
                    break;
                case ViesKeys.FaultString:
                    faultMessage = GetValue<string>(xmlReader);
                    break;
                default:
                    xmlReader.Read();
                    break;
            }
        }

        return (faultCode, faultMessage);
    }

    private static async Task<(string code, string error)> ReadErrorAsync(XmlReader xmlReader)
    {
        string faultCode = null;
        string faultMessage = null;

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            switch (xmlReader.LocalName.ToUpperInvariant())
            {
                case ViesKeys.FaultCode:
                    faultCode = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                    break;
                case ViesKeys.FaultString:
                    faultMessage = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                    break;
                default:
                    await xmlReader.ReadAsync().ConfigureAwait(false);
                    break;
            }
        }

        return (faultCode, faultMessage);
    }

    private static T GetValue<T>(XmlReader xmlReader, IFormatProvider formatProvider = null)
    {
        var val = xmlReader.ReadElementContentAsString();
        return (T) Convert.ChangeType(val, typeof(T), provider: formatProvider);
    }

    private static async Task<T> GetValueAsync<T>(XmlReader xmlReader, IFormatProvider formatProvider = null)
    {
        var val = await xmlReader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        return (T) Convert.ChangeType(val, typeof(T), provider: formatProvider);
    }
}
