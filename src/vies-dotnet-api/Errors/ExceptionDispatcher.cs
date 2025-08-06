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

namespace Padi.Vies.Errors;

internal static class ExceptionDispatcher
{
    public static void ThrowInvalidVatNumber(string param = null, string userMessage = null)
    {
        throw new ViesValidationException(
            errorCode: ViesErrorCodes.ValidationError.InvalidVatFormat.Code,
            message: ViesErrorCodes.ValidationError.InvalidVatFormat.Message,
            param: param,
            userMessage: userMessage ?? ViesErrorCodes.ValidationError.InvalidVatFormat.UserMessage
        );
    }

    public static void ThrowInvalidInput(string param = null, string userMessage = null)
    {
        throw new ViesInvalidInputException(
            errorCode: ViesErrorCodes.InputError.InvalidInput.Code,
            message: ViesErrorCodes.InputError.InvalidInput.Message,
            param: param,
            userMessage: userMessage
        );
    }

    public static void ThrowServiceUnavailable(Exception innerException = null)
    {
        throw new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
            message: ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
            innerException: innerException,
            userMessage: ViesErrorCodes.ServiceError.ServiceUnavailable.UserMessage
        );
    }

    public static void ThrowNetworkError(Exception innerException)
    {
        throw new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.NetworkError.Code,
            message: ViesErrorCodes.ServiceError.NetworkError.Message,
            innerException: innerException,
            userMessage: ViesErrorCodes.ServiceError.NetworkError.UserMessage
        );
    }

    public static void ThrowTimeout(Exception innerException)
    {
        throw new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.Timeout.Code,
            message: ViesErrorCodes.ServiceError.Timeout.Message,
            innerException: innerException,
            userMessage: ViesErrorCodes.ServiceError.Timeout.UserMessage
        );
    }

    public static void ThrowInvalidResponse(Exception innerException = null)
    {
        throw new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.InvalidResponse.Code,
            message: ViesErrorCodes.ServiceError.InvalidResponse.Message,
            innerException: innerException,
            userMessage: ViesErrorCodes.ServiceError.InvalidResponse.UserMessage
        );
    }

    public static void ThrowRateLimitExceeded(Exception innerException = null)
    {
        throw new ViesServiceException(
            errorCode: ViesErrorCodes.ServiceError.RateLimitExceeded.Code,
            message: ViesErrorCodes.ServiceError.RateLimitExceeded.Message,
            innerException: innerException,
            userMessage: ViesErrorCodes.ServiceError.RateLimitExceeded.UserMessage
        );
    }

    public static void ThrowVatNotFound(string vatNumber)
    {
        throw new ViesValidationException(
            errorCode: ViesErrorCodes.InvalidRequestError.VatNotFound.Code,
            message: $"{ViesErrorCodes.InvalidRequestError.VatNotFound.Message}: {vatNumber}.",
            param: ViesErrorCodes.InvalidRequestError.VatNotFound.Param,
            userMessage: ViesErrorCodes.InvalidRequestError.VatNotFound.UserMessage
        );
    }

    public static void ThrowUnexpectedError(Exception innerException)
    {
        throw new ViesException(
            errorCode: ViesErrorCodes.ApiError.UnexpectedError.Code,
            errorType: ViesErrorCodes.ApiError.Type,
            message: ViesErrorCodes.ApiError.UnexpectedError.Message,
            innerException: innerException,
            userMessage: ViesErrorCodes.ApiError.UnexpectedError.UserMessage
        );
    }
}
