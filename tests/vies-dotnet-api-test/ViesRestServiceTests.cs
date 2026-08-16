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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Internal;
using Xunit;

namespace Padi.Vies.Test;

#pragma warning disable CA1515
public sealed class ViesRestServiceTests
#pragma warning restore CA1515
{
    private static ViesRestService CreateService(HttpClient httpClient)
    {
        return new ViesRestService(httpClient);
    }

    [Fact]
    public async Task Should_Return_Mapped_Response_On_Ok()
    {
        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, TestPayloads.ValidJson);
        using var httpClient = new HttpClient(handler);
        ViesRestService service = CreateService(httpClient);

        ViesCheckVatResponse response = await service.SendRequestAsync("RO", "123456789", TestContext.Current.CancellationToken)
            .ConfigureAwait(true);

        Assert.True(response.IsValid);
        Assert.Equal("ACME SRL", response.TraderName);
    }

    [Fact]
    public async Task Should_Throw_ViesServiceException_On_ErrorWrappers()
    {
        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, TestPayloads.ErrorJson);
        using var httpClient = new HttpClient(handler);
        ViesRestService service = CreateService(httpClient);

        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => service.SendRequestAsync("RO", "123456789", TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.Equal(ViesErrorCodes.InputError.InvalidInput.Code, exception.ErrorCode);
    }

    [Fact]
    public async Task Should_Map_ServerError_To_ServiceUnavailable()
    {
        using var handler = new TestHttpMessageHandler(HttpStatusCode.InternalServerError, "upstream exploded");
        using var httpClient = new HttpClient(handler);
        ViesRestService service = CreateService(httpClient);

        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => service.SendRequestAsync("RO", "123456789", TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.Equal("service-unavailable", exception.ErrorCode);
    }

    [Fact]
    public async Task Should_Send_Post_Json_To_Rest_Endpoint()
    {
        HttpMethod? method = null;
        Uri? requestUri = null;
        string? contentType = null;
        string? accept = null;
        string? body = null;

        using var handler = new TestHttpMessageHandler(HttpStatusCode.OK, TestPayloads.ValidJson, (request, requestBody) =>
        {
            method = request.Method;
            requestUri = request.RequestUri;
            contentType = request.Content?.Headers.ContentType?.MediaType;
            accept = request.Headers.Accept.FirstOrDefault()?.MediaType;
            body = requestBody;
        });
        using var httpClient = new HttpClient(handler);
        ViesRestService service = CreateService(httpClient);

        await service.SendRequestAsync("RO", "123456789", TestContext.Current.CancellationToken).ConfigureAwait(true);

        Assert.Equal(HttpMethod.Post, method);
        Assert.Equal(new Uri("https://ec.europa.eu/taxation_customs/vies/rest-api/check-vat-number"), requestUri);
        Assert.Equal("application/json", contentType);
        Assert.Equal("application/json", accept);
        Assert.Equal("""{"countryCode":"RO","vatNumber":"123456789"}""", body);
    }
}
