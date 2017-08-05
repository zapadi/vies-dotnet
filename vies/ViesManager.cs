/*
   Copyright 2017 Adrian Popescu.
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
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Zapadi.Vies
{
    public class ViesManager : IDisposable
    {
        private const string VIES_URI = "http://ec.europa.eu/taxation_customs/vies/services/checkVatService";
        private const string VIES_TEST_URI = "http://ec.europa.eu/taxation_customs/vies/services/checkVatTestService";

        private const string SOAP_VALIDATE_VAT_MESSAGE_FORMAT =
                "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:v1=\"http://schemas.conversesolutions.com/xsd/dmticta/v1\"><soapenv:Header/><soapenv:Body><checkVat xmlns=\"urn:ec.europa.eu:taxud:vies:services:checkVat:types\" ><countryCode>{0}</countryCode><vatNumber>{1}</vatNumber></checkVat></soapenv:Body></soapenv:Envelope>"
            ;

        private const string TEXT_XML_MEDIA_TYPE = "text/xml";

        private readonly HttpClient httpClient;

        public ViesManager(int timeout = 10) : this(new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeout)
        })
        {
        }

        public ViesManager(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ViesCheckVatResponse> CheckIfActive(string vatNumber, CancellationToken cancellationToken)
        {
            var tuple = VatValidation.ParseVat(vatNumber);

            return await CheckIfActive(tuple.Item1, tuple.Item2, cancellationToken).ConfigureAwait(false);
        }

        private async Task<ViesCheckVatResponse> CheckIfActive(string countryCode, string vatNumber,
            CancellationToken cancellationToken)
        {
            var stringContent =
                new StringContent(string.Format(SOAP_VALIDATE_VAT_MESSAGE_FORMAT, countryCode, vatNumber),
                    Encoding.UTF8, TEXT_XML_MEDIA_TYPE);
            try
            {
                using (var postResult = await httpClient.PostAsync(VIES_URI, stringContent, cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (postResult.IsSuccessStatusCode)
                    {
                        string content = await postResult.Content.ReadAsStringAsync().ConfigureAwait(false);
                        return ParseContentResponse(content);
                    }
                    throw new ViesRequestException(
                        $"VIES service error: {postResult.StatusCode} - {postResult.ReasonPhrase}");
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                throw new Exception(httpRequestException.GetBaseException().Message, httpRequestException);
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }

        private static ViesCheckVatResponse ParseContentResponse(string content)
        {
            var faultError = content.GetValue("<faultstring>(.*?)</faultstring>");
            if (!string.IsNullOrWhiteSpace(faultError))
            {
                throw new ViesRequestException(faultError);
            }

            var cc = content.GetValue("<countryCode>(.*?)</countryCode>");
            var vn = content.GetValue("<vatNumber>(.*?)</vatNumber>");
            var n = content.GetValue("<name>(.*?)</name>");
            var adr = content.GetValue("<address>(?s)(.*?)(?-s)</address>", s => s.Replace("\\\n", "\n"));
            var isValid = content.GetValue("<valid>(true|false)</valid>");
            var dt = content.GetValue("<requestDate>(.*?)</requestDate>");
            
            return new ViesCheckVatResponse(cc, vn, dt == null ? default(DateTimeOffset) : DateTimeOffset.Parse(dt), n, adr,
                isValid != null && bool.Parse(isValid));
        }
    }
}