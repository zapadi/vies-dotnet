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

namespace Padi.Vies.Errors;

/// <summary>
/// Provides the default messages associated with each <see cref="ViesErrorCode"/> value.
/// </summary>
public static class ViesErrorMessages
{
    /// <summary>
    /// Returns the default message for the given error code.
    /// </summary>
    /// <param name="errorCode">One of the <see cref="ViesErrorCode"/> constants.</param>
    /// <returns>The default message or null when the error code is unknown.</returns>
    public static string? GetDefaultMessage(string? errorCode)
    {
        return errorCode switch
        {
            ViesErrorCode.VatInactive => ViesErrorCodes.InvalidRequestError.VatInactive.Message,
            ViesErrorCode.VatNotFound => ViesErrorCodes.InvalidRequestError.VatNotFound.Message,
            ViesErrorCode.ServiceUnavailable => ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
            ViesErrorCode.NetworkError => ViesErrorCodes.ServiceError.NetworkError.Message,
            ViesErrorCode.Timeout => ViesErrorCodes.ServiceError.Timeout.Message,
            ViesErrorCode.InvalidResponse => ViesErrorCodes.ServiceError.InvalidResponse.Message,
            ViesErrorCode.RateLimitExceeded => ViesErrorCodes.ServiceError.RateLimitExceeded.Message,
            ViesErrorCode.InvalidInput => ViesErrorCodes.InputError.InvalidInput.Message,
            ViesErrorCode.InvalidVatFormat => ViesErrorCodes.ValidationError.InvalidVatFormat.Message,
            ViesErrorCode.InvalidVatChecksumDigit => ViesErrorCodes.ValidationError.InvalidVatChecksumDigit.Message,
            ViesErrorCode.InvalidCountryCode => ViesErrorCodes.ValidationError.InvalidCountryCode.Message,
            ViesErrorCode.VatNumberTooLong => ViesErrorCodes.ValidationError.VatNumberTooLong.Message,
            ViesErrorCode.RegionUnsupported => ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Message,
            ViesErrorCode.UnexpectedError => ViesErrorCodes.ApiError.UnexpectedError.Message,
            ViesErrorCode.DeserializationFailed => ViesErrorCodes.DeserializationError.Failed.Message,
            _ => null,
        };
    }

    /// <summary>
    /// Returns the default user-facing message for the given error code.
    /// </summary>
    /// <param name="errorCode">One of the <see cref="ViesErrorCode"/> constants.</param>
    /// <returns>The default user message or null when the error code is unknown.</returns>
    public static string? GetDefaultUserMessage(string? errorCode)
    {
        return errorCode switch
        {
            ViesErrorCode.VatInactive => ViesErrorCodes.InvalidRequestError.VatInactive.UserMessage,
            ViesErrorCode.VatNotFound => ViesErrorCodes.InvalidRequestError.VatNotFound.UserMessage,
            ViesErrorCode.ServiceUnavailable => ViesErrorCodes.ServiceError.ServiceUnavailable.UserMessage,
            ViesErrorCode.NetworkError => ViesErrorCodes.ServiceError.NetworkError.UserMessage,
            ViesErrorCode.Timeout => ViesErrorCodes.ServiceError.Timeout.UserMessage,
            ViesErrorCode.InvalidResponse => ViesErrorCodes.ServiceError.InvalidResponse.UserMessage,
            ViesErrorCode.RateLimitExceeded => ViesErrorCodes.ServiceError.RateLimitExceeded.UserMessage,
            ViesErrorCode.InvalidInput => ViesErrorCodes.InputError.InvalidInput.Message,
            ViesErrorCode.InvalidVatFormat => ViesErrorCodes.ValidationError.InvalidVatFormat.UserMessage,
            ViesErrorCode.InvalidVatChecksumDigit => ViesErrorCodes.ValidationError.InvalidVatChecksumDigit.UserMessage,
            ViesErrorCode.InvalidCountryCode => ViesErrorCodes.ValidationError.InvalidCountryCode.UserMessage,
            ViesErrorCode.VatNumberTooLong => ViesErrorCodes.ValidationError.VatNumberTooLong.UserMessage,
            ViesErrorCode.RegionUnsupported => ViesErrorCodes.UnsupportedRegionError.RegionUnsupported.Message,
            ViesErrorCode.UnexpectedError => ViesErrorCodes.ApiError.UnexpectedError.UserMessage,
            ViesErrorCode.DeserializationFailed => ViesErrorCodes.DeserializationError.Failed.UserMessage,
            _ => null,
        };
    }
}
