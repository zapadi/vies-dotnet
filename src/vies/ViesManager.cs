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
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;
using Padi.Vies.Validators;

namespace Padi.Vies
{
    /// <summary>
    /// ViesManager offers a way to check if an European Union VAT is valid and/or activ.
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/VAT_identification_number#cite_note-10
    /// http://sima.cat/nif.php
    /// </remarks>
    public class ViesManager : IDisposable
    {
        private const string ViesUri = "https://ec.europa.eu/taxation_customs/vies/services/checkVatService";

        private const string SoapValidateVatMessageFormat =
            @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soapenv:Header/>
                <soapenv:Body>
                    <ns2:checkVat xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types"">
                        <ns2:countryCode>{0}</ns2:countryCode>
                        <ns2:vatNumber>{1}</ns2:vatNumber>
                    </ns2:checkVat>
                </soapenv:Body>
            </soapenv:Envelope>";

        private const string MediaTypeXml = "text/xml";

        private static readonly Dictionary<string, IVatValidator> EUVatValidators =
            new Dictionary<string, IVatValidator>
            {
                {nameof(EuCountryCode.AT), new ATVatValidator()},
                {
                    nameof(EuCountryCode.BE),
                    new BEVatValidator()
                },
                {
                    nameof(EuCountryCode.BG),
                    new BGVatValidator()
                },
                {
                    nameof(EuCountryCode.CY),
                    new CYVatValidator()
                },
                {
                    nameof(EuCountryCode.CZ),
                    new CZVatValidator()
                },
                {
                    nameof(EuCountryCode.DE),
                    new DEVatValidator()
                },
                {
                    nameof(EuCountryCode.DK),
                    new DKVatValidator()
                },
                {
                    nameof(EuCountryCode.EE),
                    new EEVatValidator()
                },
                {
                    nameof(EuCountryCode.EL),
                    new ELVatValidator()
                },
                {
                    nameof(EuCountryCode.ES),
                    new ESVatValidator()
                },
                {
                    nameof(EuCountryCode.FI),
                    new FIVatValidator()
                },
                {
                    nameof(EuCountryCode.FR),
                    new FRVatValidator()
                },
                {
                    nameof(EuCountryCode.GB),
                    new GBVatValidator()
                },
                {
                    nameof(EuCountryCode.HR),
                    new HRVatValidator()
                },
                {
                    nameof(EuCountryCode.HU),
                    new HUVatValidator()
                },
                {
                    nameof(EuCountryCode.IE),
                    new IEVatValidator()
                },
                {
                    nameof(EuCountryCode.IT),
                    new ITVatValidator()
                },
                {
                    nameof(EuCountryCode.LT),
                    new LTVatValidator()
                },
                {
                    nameof(EuCountryCode.LU),
                    new LUVatValidator()
                },
                {
                    nameof(EuCountryCode.LV),
                    new LVVatValidator()
                },
                {
                    nameof(EuCountryCode.MT),
                    new MTVatValidator()
                },
                {
                    nameof(EuCountryCode.NL),
                    new NLVatValidator()
                },
                {
                    nameof(EuCountryCode.PT),
                    new PTVatValidator()
                },
                {
                    nameof(EuCountryCode.PL),
                    new PLVatValidator()
                },
                {
                    nameof(EuCountryCode.RO),
                    new ROVatValidator()
                },
                {
                    nameof(EuCountryCode.SE),
                    new SEVatValidator()
                },
                {
                    nameof(EuCountryCode.SI),
                    new SIVatValidator()
                },
                {
                    nameof(EuCountryCode.SK),
                    new SKVatValidator()
                }
            };


        private readonly HttpClient _httpClient;
        private readonly bool _disposeClient;

        /// <summary>
        /// 
        /// </summary>
        public ViesManager() : this(
            new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            })
            {
                Timeout = TimeSpan.FromSeconds(30),
            }
            , disposeClient: true)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeXml));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="disposeClient"></param>
        public ViesManager(HttpClient httpClient, bool disposeClient = false)
        {
            _httpClient = httpClient;
            _disposeClient = disposeClient;
            _parseResponse = new XmlParseResponse();
        }

        private async Task<ViesCheckVatResponse> SendRequestAsync(string countryCode, string vatNumber, CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(ViesUri),
                Method = HttpMethod.Post,
            };

            requestMessage.Content = new StringContent(string.Format(CultureInfo.InvariantCulture, SoapValidateVatMessageFormat, countryCode, vatNumber), Encoding.UTF8, MediaTypeXml);
            requestMessage.Headers.Clear();
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeXml);
            requestMessage.Headers.Add("SOAPAction", string.Empty);

            try
            {
                using (var request = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead ,cancellationToken)
                                                      .ConfigureAwait(false))
                {
                    if (!request.IsSuccessStatusCode)
                    {
                        throw new ViesServiceException($"VIES service error: {request.StatusCode:D} - {request.ReasonPhrase}");
                    }

                    var content = await request.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    return await _parseResponse.ParseAsync(content).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                throw new Exception(httpRequestException.GetBaseException().Message, httpRequestException);
            }
        }
        
        /// <summary>
        /// Dispose the http client if disposeClient flag was set.
        /// </summary>
        public void Dispose()
        {
            if (_disposeClient)
            {
                _httpClient?.Dispose();
            }
        }

        /// <summary>
        /// Validates a VAT number
        /// </summary>
        /// <param name="vat">The VAT (with country identification) of a registered company</param>
        /// <returns>VatValidationResult</returns>
        public static VatValidationResult IsValid(string vat)
        {
            var pair = SplitInput(vat);

            return IsValid(pair.code, pair.number);
        }

        /// <summary>
        /// Validates a given country code and VAT number
        /// </summary>
        /// <param name="countryCode">The two-character country code of a European member country</param>
        /// <param name="vatNumber">The VAT number (without the country identification) of a registered company</param>
        /// <returns>VatValidationResult</returns>
        public static VatValidationResult IsValid(string countryCode, string vatNumber)
        {
            var validator = GetValidator(countryCode);
            return validator == null
                ? VatValidationResult.Failed($"{countryCode} is not a valid ISO_3166-1 European member state.")
                : validator.Validate(vatNumber);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryCode">The two-character country code of a European member country</param>
        /// <param name="vatNumber">The VAT number (without the country identification) of a registered company</param>
        /// <param name="cancellationToken"></param>
        /// <returns>ViesCheckVatResponse</returns>
        /// <exception cref="ViesValidationException"></exception>
        /// <exception cref="ViesServiceException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ViesCheckVatResponse> IsActive(string countryCode, string vatNumber, CancellationToken cancellationToken = default)
        {
            return await SendRequestAsync(countryCode, vatNumber, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vatNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>ViesCheckVatResponse</returns>
        /// <exception cref="ViesValidationException"></exception>
        /// <exception cref="ViesServiceException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ViesCheckVatResponse> IsActive(string vatNumber, CancellationToken cancellationToken = default)
        {
            var tokens = SplitInput(vatNumber);

            return await SendRequestAsync(tokens.code, tokens.number, cancellationToken).ConfigureAwait(false);
        }
        
        private static (string code, string number) SplitInput(string vat)
        {
            vat = vat.Sanitize();
            
            if (string.IsNullOrWhiteSpace(vat))
            {
                throw new ViesValidationException("VAT number cannot be null or empty.");
            }

            if (vat.Length < 3)
            {
                throw new ViesValidationException($"VAT number '{vat}' is too short.");
            }
            
            var countryCode = vat.Slice(0, 2);
            var vatNumber = vat.Slice(2);
            
            return (countryCode, vatNumber);
        }
    }
}