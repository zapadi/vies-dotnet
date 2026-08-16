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
using System.Text;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;
using Xunit;

namespace Padi.Vies.Test;

public sealed class ViesXmlDeserializationTests
{
    private readonly IResponseParser _parser = new XmlResponseParser();

    private ViesCheckVatResponse Parse(string input, Encoding encoding) => _parser.Parse(encoding.GetBytes(input));

    [Fact]
    public void Should_Deserialize_Active()
    {
        const string input = """
                             <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
                             <env:Header/>
                             <env:Body>
                                <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
                                    <ns2:countryCode>LU</ns2:countryCode>
                                    <ns2:vatNumber>26375245</ns2:vatNumber>
                                    <ns2:requestDate>2022-09-04+02:00</ns2:requestDate>
                                    <ns2:valid>true</ns2:valid>
                                    <ns2:name>AMAZON EUROPE CORE S.A R.L.</ns2:name>
                                    <ns2:address>38, AVENUE JOHN F. KENNEDY L-1855  LUXEMBOURG</ns2:address>
                              </ns2:checkVatResponse>
                             </env:Body>
                             </env:Envelope>
                             """;

        ViesCheckVatResponse response = Parse(input, Encoding.UTF32);
        Assert.True(response.IsValid);
        Assert.Equal("LU", response.CountryCode, ignoreCase: true);
        Assert.Equal("26375245", response.VatNumber, ignoreCase: true);
        Assert.Equal(response.RequestDate, new DateTimeOffset(2022, 9, 4, 0, 0, 0, new TimeSpan(2, 0, 0)));
        Assert.False(string.IsNullOrWhiteSpace(response.Address));
        Assert.False(string.IsNullOrWhiteSpace(response.Name));
    }

    [Theory]
    [InlineData(
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/>
        <env:Body>
        <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
        <ns2:countryCode>CZ</ns2:countryCode>
        <ns2:vatNumber></ns2:vatNumber>
        <ns2:requestDate>2022-09-09+02:00</ns2:requestDate>
        <ns2:valid>false</ns2:valid>
        <ns2:name></ns2:name>
        <ns2:address></ns2:address>
        </ns2:checkVatResponse>
        </env:Body>
        </env:Envelope>
        """,
        "CZ")]
    [InlineData(
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/>
        <env:Body>
        <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
        <ns2:countryCode>AT</ns2:countryCode>
        <ns2:vatNumber></ns2:vatNumber>
        <ns2:requestDate>2022-09-09+02:00</ns2:requestDate>
        <ns2:valid>false</ns2:valid>
        <ns2:name></ns2:name>
        <ns2:address></ns2:address>
        </ns2:checkVatResponse>
        </env:Body>
        </env:Envelope>
        """,
        "AT")]
    [InlineData(
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/>
        <env:Body>
        <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
        <ns2:countryCode>NL</ns2:countryCode>
        <ns2:vatNumber></ns2:vatNumber>
        <ns2:requestDate>2022-09-09+02:00</ns2:requestDate>
        <ns2:valid>false</ns2:valid>
        <ns2:name></ns2:name>
        <ns2:address></ns2:address>
        </ns2:checkVatResponse>
        </env:Body>
        </env:Envelope>
        """,
        "NL")]
    [InlineData(
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/>
        <env:Body>
        <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
        <ns2:countryCode>RO</ns2:countryCode>
        <ns2:vatNumber></ns2:vatNumber>
        <ns2:requestDate>2022-09-09+02:00</ns2:requestDate>
        <ns2:valid>false</ns2:valid>
        <ns2:name></ns2:name>
        <ns2:address></ns2:address>
        </ns2:checkVatResponse>
        </env:Body>
        </env:Envelope>
        """,
        "RO")]
    [InlineData(
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/><env:Body><ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
        <ns2:countryCode>IE</ns2:countryCode>
        <ns2:vatNumber></ns2:vatNumber>
        <ns2:requestDate>2022-09-09+02:00</ns2:requestDate>
        <ns2:valid>false</ns2:valid>
        <ns2:name></ns2:name>
        <ns2:address></ns2:address>
        </ns2:checkVatResponse>
        </env:Body>
        </env:Envelope>
        """,
        "IE")]
    public void Should_Deserialize_Inactive(string input, string countryCode)
    {
        ViesCheckVatResponse response = Parse(input, Encoding.UTF32);
        Assert.False(response.IsValid);
        Assert.True(string.IsNullOrWhiteSpace(response.Address));
        Assert.True(string.IsNullOrWhiteSpace(response.Name));
        Assert.Equal(response.RequestDate, new DateTimeOffset(2022, 9, 9, 0, 0, 0, new TimeSpan(2, 0, 0)));
        Assert.Equal(countryCode, response.CountryCode, ignoreCase: true);
    }

    [Theory]
    [InlineData(
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/>
        <env:Body>
            <env:Fault>
                <faultcode>env:Server</faultcode>
                <faultstring>INVALID_INPUT</faultstring>
            </env:Fault>
        </env:Body>
        </env:Envelope>
        """)]
    public void Should_Throw_ViesServiceException(string input)
    {
        Assert.Throws<ViesServiceException>(() => Parse(input, Encoding.UTF32));
    }

    [Fact]
    public void Should_Throw_ViesDeserializationException_On_Garbage()
    {
        Assert.Throws<ViesDeserializationException>(() => Parse("this is not xml", Encoding.UTF8));
    }

    [Fact]
    public void Should_Throw_ViesDeserializationException_On_Invalid_Bool()
    {
        const string input = """
                             <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
                             <env:Header/>
                             <env:Body>
                                <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
                                    <ns2:countryCode>LU</ns2:countryCode>
                                    <ns2:vatNumber>26375245</ns2:vatNumber>
                                    <ns2:requestDate>2022-09-04+02:00</ns2:requestDate>
                                    <ns2:valid>YES</ns2:valid>
                                    <ns2:name>ACME</ns2:name>
                                    <ns2:address>ADDR</ns2:address>
                              </ns2:checkVatResponse>
                             </env:Body>
                             </env:Envelope>
                             """;

        Assert.Throws<ViesDeserializationException>(() => Parse(input, Encoding.UTF8));
    }
}
