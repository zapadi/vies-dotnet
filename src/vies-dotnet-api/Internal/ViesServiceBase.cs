/*
   Copyright 2017-2025 Adrian Popescu.
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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;
using Padi.Vies.Parsers;

namespace Padi.Vies.Internal;

internal abstract class ViesServiceBase(HttpClient httpClient, IResponseParserAsync responseParser) : IViesService
{
    protected IResponseParserAsync ResponseParser { get; } = responseParser;

    public async Task<ViesCheckVatResponse> SendRequestAsync(string countryCode, string vatNumber, CancellationToken cancellationToken)
    {
        try
        {
            using (HttpRequestMessage requestMessage = CreateHttpRequestMessage(countryCode, vatNumber))
            using (HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                {
                    return await HandleNonSuccessAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
                }

                using (Stream stream =
#if (NETSTANDARD2_0 || NETSTANDARD2_1)
                    await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
#else
                    await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
#endif
                {
                    return await ResponseParser.ParseAsync(stream, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            throw ViesHttpErrorHandler.CreateServiceUnavailableException(httpRequestException);
        }
        catch (OperationCanceledException operationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout surfaces as TaskCanceledException without the caller token being canceled.
            throw ViesHttpErrorHandler.CreateTimeoutException(operationCanceledException);
        }
        catch (IOException ioException)
        {
            throw ViesHttpErrorHandler.CreateNetworkErrorException(ioException);
        }
    }

    protected abstract HttpRequestMessage CreateHttpRequestMessage(string countryCode, string vatNumber);

    protected virtual async Task<ViesCheckVatResponse> HandleNonSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        throw await ViesHttpErrorHandler.CreateExceptionAsync(response, cancellationToken).ConfigureAwait(false);
    }
}
