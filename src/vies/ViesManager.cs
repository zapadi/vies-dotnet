/*
   Copyright 2017-2019 Adrian Popescu.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private const string ViesUri = "http://ec.europa.eu/taxation_customs/vies/services/checkVatService";

        private const string SoapValidateVatMessageFormat =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soapenv:Header/>
                <soapenv:Body>
                    <checkVat xmlns=""urn:ec.europa.eu:taxud:vies:services:checkVat:types"">
                        <countryCode>{0}</countryCode>
                        <vatNumber>{1}</vatNumber>
                    </checkVat>
                </soapenv:Body>
            </soapenv:Envelope>";

        private const string MediaTypeXml = "text/xml";

        private static readonly Dictionary<string, IVatValidator> EUVatValidators =
            new Dictionary<string, IVatValidator>()
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
            new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            })
            {
                Timeout = TimeSpan.FromSeconds(30)
            }
            , true)
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
        }

        private async Task<ViesCheckVatResponse> CheckIfActive(string countryCode, string vatNumber,
            CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(ViesUri),
                Method = HttpMethod.Post
            };

            requestMessage.Content =
                new StringContent(string.Format(SoapValidateVatMessageFormat, countryCode, vatNumber), Encoding.UTF8,
                    MediaTypeXml);
            requestMessage.Headers.Clear();
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeXml);
            requestMessage.Headers.Add("SOAPAction", "urn:ec.europa.eu:taxud:vies:services:checkVat");

            try
            {
                using (var request = await _httpClient.SendAsync(requestMessage, cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (!request.IsSuccessStatusCode)
                        throw new ViesServiceException(
                            $"VIES service error: {request.StatusCode:D} - {request.ReasonPhrase}");

                    string content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return ParseContentResponse(content);
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
            vat = vat.Sanitize();

            if (string.IsNullOrWhiteSpace(vat))
                return VatValidationResult.Failed("VAT number cannot be null or empty.");

            if (vat.Length < 3)
                return VatValidationResult.Failed($"VAT number '{vat}' is too short.");

            var countryCode = vat.Substring(0, 2);

            if (!EUVatValidators.TryGetValue(countryCode, out var validator))
                return VatValidationResult.Failed($"{countryCode} is not a valid ISO_3166-1 European member state.");

            var vatNumber = vat.Substring(2);
            return validator.Validate(vatNumber);
        }

        /// <summary>
        /// Validates a given country code and VAT number
        /// </summary>
        /// <param name="countryCode">The two-character country code of a European member country</param>
        /// <param name="vatNumber">The VAT number (without the country identification) of a registered company</param>
        /// <returns>VatValidationResult</returns>
        public static VatValidationResult IsValid(string countryCode, string vatNumber)
        {
            return !EUVatValidators.TryGetValue(countryCode, out var validator)
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
        public async Task<ViesCheckVatResponse> IsActive(string countryCode, string vatNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
                throw new ViesValidationException("Country code cannot be null.");

            if (!EUVatValidators.TryGetValue(countryCode, out var validator))
                throw new ViesValidationException($"{countryCode} is not a valid ISO_3166-1 European member state.");

            vatNumber = vatNumber.Sanitize();
            var validationResult = validator.Validate(vatNumber);

            if (!validationResult.IsValid)
            {
                throw new ViesValidationException($"'{countryCode}{vatNumber}' is invalid.");
            }

            return await CheckIfActive(countryCode, vatNumber, cancellationToken);
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
        public async Task<ViesCheckVatResponse> IsActive(string vatNumber,
            CancellationToken cancellationToken = default)
        {
            vatNumber = vatNumber.Sanitize();

            var validationResult = IsValid(vatNumber);

            if (!validationResult.IsValid)
            {
                throw new ViesValidationException($"'{vatNumber}' is invalid.");
            }

            return await CheckIfActive(vatNumber.Substring(0, 2), vatNumber.Substring(2), cancellationToken);
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
        private static ViesCheckVatResponse ParseContentResponse(string content)
        {
            var faultError = content.GetValue("<faultstring>(.*?)</faultstring>");
            if (!string.IsNullOrWhiteSpace(faultError))
            {
                throw new ViesServiceException(faultError);
            }

            var cc = content.GetValue("<countryCode>(.*?)</countryCode>");
            var vn = content.GetValue("<vatNumber>(.*?)</vatNumber>");
            var n = content.GetValue("<name>(.*?)</name>");
            var adr = content.GetValue("<address>(?s)(.*?)(?-s)</address>", s => s.Replace("\\\n", "\n"));
            var isValid = content.GetValue("<valid>(true|false)</valid>");
            var dt = content.GetValue("<requestDate>(.*?)</requestDate>");

            return new ViesCheckVatResponse(cc, vn, dt == null ? default(DateTimeOffset) : DateTimeOffset.Parse(dt), n,
                adr,
                isValid != null && bool.Parse(isValid));
        }
    }
}