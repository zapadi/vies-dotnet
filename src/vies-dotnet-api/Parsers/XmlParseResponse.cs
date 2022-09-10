/*
   Copyright 2017-2022 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Padi.Vies.Errors;

namespace Padi.Vies.Parsers
{
    public sealed class XmlParseResponse : IParseResponseAsync
    {
        private static readonly XmlReaderSettings xmlReaderSettings = new XmlReaderSettings()
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
            using (var xmlReader = XmlReader.Create(response, xmlReaderSettings))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if (ViesKeys.FAULT.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        var fault = ReadError(xmlReader);
                        throw new ViesServiceException(fault.error);
                    }

                    if (!ViesKeys.CHECK_VAT_RESPONSE.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
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
            using (var xmlReader = XmlReader.Create(response, xmlReaderSettings))
            {
                while (await xmlReader.ReadAsync().ConfigureAwait(false))
                {
                    if (xmlReader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    if (ViesKeys.FAULT.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        var fault = await ReadErrorAsync(xmlReader).ConfigureAwait(false);
                        throw new ViesServiceException(fault.error);
                    }

                    if (!ViesKeys.CHECK_VAT_RESPONSE.Equals(xmlReader.LocalName, StringComparison.OrdinalIgnoreCase))
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
                    case ViesKeys.COUNTRY_CODE:
                        viesCheckVatResponse.CountryCode = GetValue<string>(xmlReader);
                        break;
                    case ViesKeys.VAT_NUMBER:
                        viesCheckVatResponse.VatNumber = GetValue<string>(xmlReader);
                        break;
                    case ViesKeys.REQUEST_DATE:
                        var dt = GetValue<DateTime>(xmlReader, CultureInfo.InvariantCulture);
                        viesCheckVatResponse.RequestDate =new DateTimeOffset(dt); 
                        break;
                    case ViesKeys.VALID:
                        viesCheckVatResponse.IsValid = GetValue<bool>(xmlReader);
                        break;
                    case ViesKeys.NAME:
                        viesCheckVatResponse.Name = GetValue<string>(xmlReader);
                        break;
                    case ViesKeys.ADDRESS:
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
                    case ViesKeys.COUNTRY_CODE:
                        viesCheckVatResponse.CountryCode = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                        break;
                    case ViesKeys.VAT_NUMBER:
                        viesCheckVatResponse.VatNumber = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                        break;
                    case ViesKeys.REQUEST_DATE:
                        var dt = await GetValueAsync<DateTime>(xmlReader, CultureInfo.InvariantCulture).ConfigureAwait(false);
                        viesCheckVatResponse.RequestDate = new DateTimeOffset(dt);
                        break;
                    case ViesKeys.VALID:
                        viesCheckVatResponse.IsValid = await GetValueAsync<bool>(xmlReader).ConfigureAwait(false);
                        break;
                    case ViesKeys.NAME:
                        viesCheckVatResponse.Name = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                        break;
                    case ViesKeys.ADDRESS:
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
                    case ViesKeys.FAULT_CODE:
                        faultCode = GetValue<string>(xmlReader);
                        break;
                    case ViesKeys.FAULT_STRING:
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
                    case ViesKeys.FAULT_CODE:
                        faultCode = await GetValueAsync<string>(xmlReader).ConfigureAwait(false);
                        break;
                    case ViesKeys.FAULT_STRING:
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
        
        
        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="ViesServiceException">
        /// 200 = Valid request with an Invalid VAT Number
        /// 201 = Error : INVALID_INPUT
        /// 202 = Error : INVALID_REQUESTER_INFO
        /// 300 = Error : SERVICE_UNAVAILABLE
        /// 301 = Error : MS_UNAVAILABLE
        /// 302 = Error : TIMEOUT
        /// 400 = Error : VAT_BLOCKED
        /// 401 = Error : IP_BLOCKED
        /// 500 = Error : GLOBAL_MAX_CONCURRENT_REQ
        /// 501 = Error : GLOBAL_MAX_CONCURRENT_REQ_TIME
        /// 600 = Error : MS_MAX_CONCURRENT_REQ
        /// 601 = Error : MS_MAX_CONCURRENT_REQ_TIME
        /// For all the other cases, The web service will responds with a "SERVICE_UNAVAILABLE" error.  
        /// </exception>
    }
}