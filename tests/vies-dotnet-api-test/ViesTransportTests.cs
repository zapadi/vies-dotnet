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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Internal;
using Xunit;

namespace Padi.Vies.Test;

#pragma warning disable CA1515
public sealed class ViesTransportTests
#pragma warning restore CA1515
{
    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw exception;
        }
    }

    private static async Task<ViesCheckVatResponse> SendAsync(HttpMessageHandler handler, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient(handler);
        var service = new ViesRestService(httpClient);
        return await service.SendRequestAsync("RO", "123456789", cancellationToken).ConfigureAwait(true);
    }

    [Fact]
    public async Task Should_Map_TaskCanceled_Without_Token_To_Timeout()
    {
        var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("timeout"));

        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync(handler, TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.Equal(ViesErrorCodes.ServiceError.Timeout.Code, exception.ErrorCode);
    }

    [Fact]
    public async Task Should_Map_IOException_To_NetworkError()
    {
        var handler = new ThrowingHttpMessageHandler(new IOException("socket reset"));

        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync(handler, TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.Equal(ViesErrorCodes.ServiceError.NetworkError.Code, exception.ErrorCode);
    }

    [Fact]
    public async Task Should_Propagate_OperationCanceled_When_Token_Canceled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync().ConfigureAwait(true);

        var handler = new ThrowingHttpMessageHandler(new TaskCanceledException("canceled"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => SendAsync(handler, cts.Token)).ConfigureAwait(true);
    }

    [Fact]
    public async Task Should_Map_HttpRequestException_To_ServiceUnavailable_Preserving_Inner()
    {
        var inner = new HttpRequestException("connection refused");
        var handler = new ThrowingHttpMessageHandler(inner);

        ViesServiceException exception = await Assert.ThrowsAsync<ViesServiceException>(
            () => SendAsync(handler, TestContext.Current.CancellationToken)).ConfigureAwait(true);

        Assert.Equal(ViesErrorCodes.ServiceError.ServiceUnavailable.Code, exception.ErrorCode);
        Assert.Same(inner, exception.InnerException);
    }
}
