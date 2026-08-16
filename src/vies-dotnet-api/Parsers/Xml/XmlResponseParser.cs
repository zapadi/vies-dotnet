/*
   Copyright 2017-2026 Adrian Popescu.
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
using System.Runtime.InteropServices;
using System.Xml;
using Padi.Vies.Errors;
using Padi.Vies.Internal;
using Padi.Vies.Internal.Extensions;

namespace Padi.Vies.Parsers;

internal sealed class XmlResponseParser : IResponseParser
{
    private static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore,
        CheckCharacters = false,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        CloseInput = true,
    };

    public ViesCheckVatResponse Parse(ReadOnlyMemory<byte> response)
    {
        using (Stream stream = CreateStream(response))
        using (var xmlReader = XmlReader.Create(stream, XmlReaderSettings))
        {
            try
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
                        var (faultCode, faultMessage) = ReadError(xmlReader);
                        throw CreateFaultException(faultCode, faultMessage);
                    }

                    if (!ViesKeys.CheckVatResponse.AsSpan().Equals(localName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    return ReadResponse(xmlReader);
                }
            }
            catch (Exception ex) when (ex is XmlException or InvalidCastException or FormatException or OverflowException)
            {
                return ExceptionDispatcher.ThrowDeserialization<ViesCheckVatResponse>(ex);
            }
        }

        return ExceptionDispatcher.ThrowDeserialization<ViesCheckVatResponse>();
    }

    private static MemoryStream CreateStream(ReadOnlyMemory<byte> response)
    {
        if (MemoryMarshal.TryGetArray(response, out ArraySegment<byte> segment) && segment.Array is not null)
        {
            return new MemoryStream(segment.Array, segment.Offset, segment.Count, writable: false);
        }

        return new MemoryStream(response.ToArray(), writable: false);
    }

    private static ViesServiceException CreateFaultException(string? faultCode, string? faultMessage)
    {
        var (code, message, userMessage) = ViesErrorCodeMapper.Map(faultMessage);
        return new ViesServiceException(code, InvariantString.Format($"{message} (VIES fault {faultCode}: {faultMessage})."), userMessage: userMessage);
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

    private static (string? code, string? error) ReadError(XmlReader xmlReader)
    {
        string? faultCode = null, faultMessage = null;

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
}
