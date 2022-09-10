/*
   Copyright 2017-2022 Adrian Popescu.
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
using System.Text;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;
using Xunit;

namespace Padi.Vies.Test
{
    public class ViesXmlDeserializationAsyncTests
    {
        private readonly IParseResponseAsync _parseResponseAsync = new XmlParseResponse();

        [Fact]
        public async Task Should_Deserialize_Active()
        {
            var input = @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">
         <env:Header/>
        <env:Body>
              <ns2:checkVatResponse xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types"">
                  <ns2:countryCode>LU</ns2:countryCode>
                  <ns2:vatNumber>26375245</ns2:vatNumber>
                  <ns2:requestDate>2022-09-04+02:00</ns2:requestDate>
                  <ns2:valid>true</ns2:valid>
                  <ns2:name>AMAZON EUROPE CORE S.A R.L.</ns2:name>
                  <ns2:address>38, AVENUE JOHN F. KENNEDY L-1855  LUXEMBOURG</ns2:address>
            </ns2:checkVatResponse>
         </env:Body>
         </env:Envelope>";

            using (var stream = new MemoryStream(Encoding.UTF32.GetBytes(input)))
            {
                var response = await _parseResponseAsync.ParseAsync(stream);
                Assert.True(response.IsValid);
                Assert.True("LU".Equals(response.CountryCode, StringComparison.OrdinalIgnoreCase));
                Assert.True("26375245".Equals(response.VatNumber, StringComparison.OrdinalIgnoreCase));
                Assert.Equal(response.RequestDate, new DateTimeOffset(2022, 9, 4, 0, 0, 0, new TimeSpan(2, 0, 0)));
                Assert.True(!string.IsNullOrWhiteSpace(response.Address));
                Assert.True(!string.IsNullOrWhiteSpace(response.Name));
            }
        }

        [Theory]
        [InlineData(
            @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header/><env:Body><ns2:checkVatResponse xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types""><ns2:countryCode>CZ</ns2:countryCode><ns2:vatNumber></ns2:vatNumber><ns2:requestDate>2022-09-09+02:00</ns2:requestDate><ns2:valid>false</ns2:valid><ns2:name></ns2:name><ns2:address></ns2:address></ns2:checkVatResponse></env:Body></env:Envelope>",
            "CZ")]
        [InlineData(
            @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header/><env:Body><ns2:checkVatResponse xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types""><ns2:countryCode>AT</ns2:countryCode><ns2:vatNumber></ns2:vatNumber><ns2:requestDate>2022-09-09+02:00</ns2:requestDate><ns2:valid>false</ns2:valid><ns2:name></ns2:name><ns2:address></ns2:address></ns2:checkVatResponse></env:Body></env:Envelope>",
            "AT")]
        [InlineData(
            @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header/><env:Body><ns2:checkVatResponse xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types""><ns2:countryCode>NL</ns2:countryCode><ns2:vatNumber></ns2:vatNumber><ns2:requestDate>2022-09-09+02:00</ns2:requestDate><ns2:valid>false</ns2:valid><ns2:name></ns2:name><ns2:address></ns2:address></ns2:checkVatResponse></env:Body></env:Envelope>",
            "NL")]
        [InlineData(
            @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header/><env:Body><ns2:checkVatResponse xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types""><ns2:countryCode>RO</ns2:countryCode><ns2:vatNumber></ns2:vatNumber><ns2:requestDate>2022-09-09+02:00</ns2:requestDate><ns2:valid>false</ns2:valid><ns2:name></ns2:name><ns2:address></ns2:address></ns2:checkVatResponse></env:Body></env:Envelope>",
            "RO")]
        [InlineData(
            @"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header/><env:Body><ns2:checkVatResponse xmlns:ns2=""urn:ec.europa.eu:taxud:vies:services:checkVat:types""><ns2:countryCode>IE</ns2:countryCode><ns2:vatNumber></ns2:vatNumber><ns2:requestDate>2022-09-09+02:00</ns2:requestDate><ns2:valid>false</ns2:valid><ns2:name></ns2:name><ns2:address></ns2:address></ns2:checkVatResponse></env:Body></env:Envelope>",
            "IE")]
        public async Task Should_Deserialize_Inactive(string input, string countryCode)
        {
            using (var stream = new MemoryStream(Encoding.UTF32.GetBytes(input)))
            {
                var response = await _parseResponseAsync.ParseAsync(stream);
                Assert.False(response.IsValid);
                Assert.True(string.IsNullOrWhiteSpace(response.Address));
                Assert.True(string.IsNullOrWhiteSpace(response.Name));
                Assert.Equal(response.RequestDate, new DateTimeOffset(2022, 9, 9, 0, 0, 0, new TimeSpan(2, 0, 0)));
                Assert.True(countryCode.Equals(response.CountryCode, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Theory]
        [InlineData(@"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/""><env:Header/><env:Body><env:Fault><faultcode>env:Server</faultcode><faultstring>INVALID_INPUT</faultstring></env:Fault></env:Body></env:Envelope>")]
        public async Task Should_Throw_ViesServiceException(string input)
        {
            using (var stream = new MemoryStream(Encoding.UTF32.GetBytes(input)))
            {
                await Assert.ThrowsAsync<ViesServiceException>(() => _parseResponseAsync.ParseAsync(stream));
            }
        }
    }
}