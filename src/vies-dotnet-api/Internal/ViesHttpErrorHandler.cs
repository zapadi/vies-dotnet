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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Padi.Vies.Errors;

namespace Padi.Vies.Internal;

internal static class ViesHttpErrorHandler
{
    private const int MaxErrorBodyLength = 512;
    private const int MaxErrorBodyReadBytes = 2048;

    /// <summary>
    /// Builds a <see cref="ViesServiceException"/> from a non-success HTTP response, mapping the HTTP status code to a library error code.
    /// </summary>
    public static async Task<ViesServiceException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorBody = await ReadBoundedBodyAsync(response, MaxErrorBodyReadBytes, cancellationToken).ConfigureAwait(false);
        return CreateException(response.StatusCode, response.ReasonPhrase, errorBody);
    }

    public static ViesServiceException CreateException(HttpStatusCode statusCode, string reasonPhrase, string errorBody)
    {
        var (errorCode, message, userMessage) = MapServiceError(statusCode);

        var detail = InvariantString.Format($"HTTP {(int)statusCode} {reasonPhrase}");
        if (string.IsNullOrWhiteSpace(errorBody))
        {
            return new ViesServiceException(
                errorCode: errorCode,
                message: InvariantString.Format($"{message} ({detail})."),
                userMessage: userMessage
            );
        }

        var body = errorBody.Length > MaxErrorBodyLength ? errorBody.Substring(0, MaxErrorBodyLength) : errorBody;
        detail = InvariantString.Format($"{detail}: {body}");

        return new ViesServiceException(
            errorCode: errorCode,
            message: InvariantString.Format($"{message} ({detail})."),
            userMessage: userMessage
        );
    }

    private static async Task<string> ReadBoundedBodyAsync(HttpResponseMessage response, int maxBytes, CancellationToken cancellationToken)
    {
        using (Stream stream =
#if (NETSTANDARD2_0 || NETSTANDARD2_1)
            await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
#else
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
#endif
        {
            var buffer = new byte[maxBytes];
            var total = 0;

            while (total < maxBytes)
            {
                var read = await stream.ReadAsync(
#if NETSTANDARD2_0
                    buffer, total, maxBytes - total, cancellationToken).ConfigureAwait(false);
#else
                    buffer.AsMemory(total, maxBytes - total), cancellationToken).ConfigureAwait(false);
#endif
                if (read == 0)
                {
                    break;
                }

                total += read;
            }

            return Encoding.UTF8.GetString(buffer, 0, total);
        }
    }

    public static ViesServiceException CreateServiceUnavailableException(HttpRequestException httpRequestException)
    {
        return new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
            message: ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
            userMessage: httpRequestException.GetBaseException().Message,
            innerException: httpRequestException
        );
    }

    public static ViesServiceException CreateTimeoutException(Exception innerException)
    {
        return new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.Timeout.Code,
            message: ViesErrorCodes.ServiceError.Timeout.Message,
            userMessage: ViesErrorCodes.ServiceError.Timeout.UserMessage,
            innerException: innerException
        );
    }

    public static ViesServiceException CreateNetworkErrorException(IOException ioException)
    {
        return new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.NetworkError.Code,
            message: ViesErrorCodes.ServiceError.NetworkError.Message,
            userMessage: ViesErrorCodes.ServiceError.NetworkError.UserMessage,
            innerException: ioException
        );
    }

    private static (string Code, string Message, string UserMessage) MapServiceError(HttpStatusCode statusCode)
    {
        return (int)statusCode switch
        {
            429 => (ViesErrorCodes.ServiceError.RateLimitExceeded.Code, ViesErrorCodes.ServiceError.RateLimitExceeded.Message, ViesErrorCodes.ServiceError.RateLimitExceeded.UserMessage),
            408 or 504 => (ViesErrorCodes.ServiceError.Timeout.Code, ViesErrorCodes.ServiceError.Timeout.Message, ViesErrorCodes.ServiceError.Timeout.UserMessage),
            _ => (ViesErrorCodes.ServiceError.ServiceUnavailable.Code, ViesErrorCodes.ServiceError.ServiceUnavailable.Message, ViesErrorCodes.ServiceError.ServiceUnavailable.UserMessage),
        };
    }
}
