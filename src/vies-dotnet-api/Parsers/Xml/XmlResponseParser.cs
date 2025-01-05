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

                ReadOnlySpan<char> localName = xmlReader.LocalName.AsSpan();
                if (ViesKeys.Fault.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
                {
                    var (_, error) = ReadError(xmlReader);
                    throw new ViesServiceException(error);
                }

                if (!ViesKeys.CheckVatResponse.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
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
                    var (_, error) = await ReadErrorAsync(xmlReader).ConfigureAwait(false);
                    throw new ViesServiceException(error);
                }

                if (!ViesKeys.CheckVatResponse.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return await ReadResponseAsync(xmlReader).ConfigureAwait(false);
            }
        }

        throw new ViesDeserializationException($"Could not deserialize response: {response}");
    }

    private static ViesCheckVatResponse ReadResponse(XmlReader xmlReader)
    {
        var viesCheckVatResponse = new ViesCheckVatResponse();

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            ReadOnlySpan<char> localName = xmlReader.LocalName.AsSpan();
            if (localName.Equals(ViesKeys.CountryCode.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.CountryCode = xmlReader.GetValueAsString();
            }
            else if (localName.Equals(ViesKeys.VatNumber.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.VatNumber = xmlReader.GetValueAsString();
            }
            else if (localName.Equals(ViesKeys.RequestDate.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.RequestDate = xmlReader.GetValueAsDateTimeOffset();
            }
            else if (localName.Equals(ViesKeys.Valid.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.IsValid = xmlReader.GetValueAsBool();
            }
            else if (localName.Equals(ViesKeys.Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.Name = xmlReader.GetValueAsString();
            }
            else if (localName.Equals(ViesKeys.Address.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.Address = xmlReader.GetValueAsString();
            }
            else
            {
                xmlReader.Read();
            }
        }

        return viesCheckVatResponse;
    }

    private static async Task<ViesCheckVatResponse> ReadResponseAsync(XmlReader xmlReader)
    {
        var viesCheckVatResponse = new ViesCheckVatResponse();

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            ReadOnlySpan<char> localName = xmlReader.LocalName.AsSpan();
            if (localName.Equals(ViesKeys.CountryCode.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.CountryCode = await xmlReader.GetValueAsStringAsync().ConfigureAwait(false);
            }
            else if (localName.Equals(ViesKeys.VatNumber.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.VatNumber = await xmlReader.GetValueAsStringAsync().ConfigureAwait(false);
            }
            else if (localName.Equals(ViesKeys.RequestDate.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.RequestDate = await xmlReader.GetValueAsDateTimeOffsetAsync().ConfigureAwait(false);
            }
            else if (localName.Equals(ViesKeys.Valid.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.IsValid = await xmlReader.GetValueAsBoolAsync().ConfigureAwait(false);
            }
            else if (localName.Equals(ViesKeys.Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.Name = await xmlReader.GetValueAsStringAsync().ConfigureAwait(false);
            }
            else if (localName.Equals(ViesKeys.Address.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                viesCheckVatResponse.Address = await xmlReader.GetValueAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                await xmlReader.ReadAsync().ConfigureAwait(false);
            }
        }

        return viesCheckVatResponse;
    }

    private static (string code, string error) ReadError(XmlReader xmlReader)
    {
        string faultCode = null, faultMessage = null;

        while (xmlReader.NodeType == XmlNodeType.Element)
        {
            ReadOnlySpan<char> localName = xmlReader.LocalName.AsSpan();

            if (ViesKeys.FaultCode.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
            {
                faultCode = xmlReader.GetValueAsString();
            }
            else
            {
                if (ViesKeys.FaultString.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
                {
                    faultMessage = xmlReader.GetValueAsString();
                }
                else
                {
                    xmlReader.Read();
                }
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
            ReadOnlySpan<char> localName = xmlReader.LocalName.AsSpan();

            if (ViesKeys.FaultCode.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
            {
                faultCode = await xmlReader.GetValueAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                if (ViesKeys.FaultString.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
                {
                    faultMessage = await xmlReader.GetValueAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    await xmlReader.ReadAsync().ConfigureAwait(false);
                }
            }
        }

        return (faultCode, faultMessage);
    }
}
