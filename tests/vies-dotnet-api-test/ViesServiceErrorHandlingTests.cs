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
using Padi.Vies.Internal;
using Xunit;

namespace Padi.Vies.Test;

#pragma warning disable CA1515
public sealed class ViesServiceErrorHandlingTests
#pragma warning restore CA1515
{
    private static async Task<ViesCheckVatResponse> SendAsync(HttpStatusCode statusCode, string content)
    {
        using var httpClient = new HttpClient(new TestHttpMessageHandler(statusCode, content));
        var service = new ViesService(httpClient);
        return await service.SendRequestAsync("RO", "123456789", TestContext.Current.CancellationToken).ConfigureAwait(true);
    }

    [Theory]
    [InlineData((int)HttpStatusCode.InternalServerError, "service-unavailable")]
    [InlineData(429, "rate-limit-exceeded")]
    [InlineData((int)HttpStatusCode.RequestTimeout, "timeout")]
    [InlineData((int)HttpStatusCode.GatewayTimeout, "timeout")]
    [InlineData((int)HttpStatusCode.BadGateway, "service-unavailable")]
    public async Task Should_Map_StatusCode_To_ExpectedErrorCode(int statusCode, string expectedErrorCode)
    {
        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync((HttpStatusCode)statusCode, "upstream exploded")).ConfigureAwait(true);

        Assert.Equal(expectedErrorCode, exception.ErrorCode);
    }

    [Fact]
    public async Task Should_Propagate_StatusCode_And_Body_In_Message()
    {
        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync(HttpStatusCode.InternalServerError, "MS_MAX_CONCURRENT_REQ")).ConfigureAwait(true);

        Assert.Contains("500", exception.Message, StringComparison.Ordinal);
        Assert.Contains("MS_MAX_CONCURRENT_REQ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Should_Truncate_Long_Body_In_Message()
    {
        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync(HttpStatusCode.InternalServerError, new string('x', 2000))).ConfigureAwait(true);

        Assert.True(exception.Message.Length < 700);
    }

    [Fact]
    public async Task Should_Recover_SoapFault_On_Http500_And_Map_ErrorCode()
    {
        const string soapFault =
            """
            <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
            <env:Header/>
            <env:Body>
                <env:Fault>
                    <faultcode>env:Server</faultcode>
                    <faultstring>MS_MAX_CONCURRENT_REQ</faultstring>
                </env:Fault>
            </env:Body>
            </env:Envelope>
            """;

        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync(HttpStatusCode.InternalServerError, soapFault)).ConfigureAwait(true);

        Assert.Equal(ViesErrorCodes.ServiceError.RateLimitExceeded.Code, exception.ErrorCode);
    }
}
