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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Xunit;

namespace Padi.Vies.Test;

#pragma warning disable CA1515
public sealed class ViesManagerWiringTests
#pragma warning restore CA1515
{
    [Fact]
    public async Task Should_Post_Xml_To_Soap_Endpoint()
    {
        HttpMethod? method = null;
        Uri? requestUri = null;
        string? contentType = null;

        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, SoapSuccess, (request, _) =>
        {
            method = request.Method;
            requestUri = request.RequestUri;
            contentType = request.Content?.Headers.ContentType?.ToString();
        });
        using var httpClient = new HttpClient(handler);
        using var manager = new ViesManager(httpClient, disposeClient: false, ViesApiEndpoint.Soap);

        await manager.IsActiveAsync("LU", "26375245", TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal(HttpMethod.Post, method);
        Assert.Equal(new Uri("https://ec.europa.eu/taxation_customs/vies/services/checkVatService"), requestUri);
        Assert.Equal("text/xml; charset=utf-8", contentType);
    }

    [Fact]
    public async Task Should_Post_Json_To_Rest_Endpoint()
    {
        Uri? requestUri = null;
        string? contentType = null;

        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, TestPayloads.ValidJson, (request, _) =>
        {
            requestUri = request.RequestUri;
            contentType = request.Content?.Headers.ContentType?.ToString();
        });
        using var httpClient = new HttpClient(handler);
        using var manager = new ViesManager(httpClient, disposeClient: false, ViesApiEndpoint.Rest);

        await manager.IsActiveAsync("RO", "123456789", TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal(new Uri("https://ec.europa.eu/taxation_customs/vies/rest-api/check-vat-number"), requestUri);
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task Should_Send_EL_CountryCode_When_GR_Alias_Used()
    {
        string? body = null;

        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, TestPayloads.ValidJson, (_, requestBody) =>
        {
            body = requestBody;
        });
        using var httpClient = new HttpClient(handler);
        using var manager = new ViesManager(httpClient, disposeClient: false, ViesApiEndpoint.Rest);

        await manager.IsActiveAsync("GR", "123456789", TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Contains("\"countryCode\":\"EL\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_Throw_ViesUnsupportedRegionException_For_Excluded_Country()
    {
        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, TestPayloads.ValidJson);
        using var httpClient = new HttpClient(handler);
        using var manager = new ViesManager(httpClient, disposeClient: false, ViesApiEndpoint.Rest);

        await Assert.ThrowsAsync<ViesUnsupportedRegionException>(
            () => manager.IsActiveAsync("GB434031494", TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.Equal(0, handler.RequestCount);
    }

    private const string SoapSuccess =
        """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
        <env:Header/>
        <env:Body>
           <ns2:checkVatResponse xmlns:ns2="urn:ec.europa.eu:taxud:vies:services:checkVat:types">
               <ns2:countryCode>LU</ns2:countryCode>
               <ns2:vatNumber>26375245</ns2:vatNumber>
               <ns2:requestDate>2022-09-04+02:00</ns2:requestDate>
               <ns2:valid>true</ns2:valid>
               <ns2:name>ACME</ns2:name>
               <ns2:address>ADDR</ns2:address>
         </ns2:checkVatResponse>
        </env:Body>
        </env:Envelope>
        """;
}
