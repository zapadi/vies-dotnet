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

namespace Padi.Vies.Errors;

internal static class ViesErrorCodeMapper
{
    /// <summary>
    /// Translates the official VIES fault strings into stable library error codes.
    /// 200 = Valid request with an Invalid VAT Number
    /// 201 = Error : INVALID_INPUT
    /// 202 = Error : INVALID_REQUESTER_INFO
    /// 300 = Error : SERVICE_UNAVAILABLE
    /// 301 = Error : MS_UNAVAILABLE
    /// 302 = Error : TIMEOUT
    /// 400 = Error : VAT_BLOCKED
    /// 401 = Error : IP_BLOCKED
    /// 500 = Error : GLOBAL_MAX_CONCURRENT_REQ
    /// 501 = Error : GLOBAL_MAX_CONCURRENT_REQ_TIME
    /// 600 = Error : MS_MAX_CONCURRENT_REQ
    /// 601 = Error : MS_MAX_CONCURRENT_REQ_TIME
    /// For all the other cases, the web service will respond with a "SERVICE_UNAVAILABLE" error.
    /// </summary>
    public static (string Code, string Message, string UserMessage) Map(string viesFault)
    {
        if (string.IsNullOrWhiteSpace(viesFault))
        {
            return (
                ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
                ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
                ViesErrorCodes.ServiceError.ServiceUnavailable.UserMessage);
        }

        if (Equals(viesFault, "INVALID_INPUT") || Equals(viesFault, "INVALID_REQUESTER_INFO"))
        {
            return (
                ViesErrorCodes.InputError.InvalidInput.Code,
                ViesErrorCodes.InputError.InvalidInput.Message,
                viesFault);
        }

        if (Equals(viesFault, "SERVICE_UNAVAILABLE") || Equals(viesFault, "MS_UNAVAILABLE"))
        {
            return (
                ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
                ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
                viesFault);
        }

        if (Equals(viesFault, "TIMEOUT"))
        {
            return (
                ViesErrorCodes.ServiceError.Timeout.Code,
                ViesErrorCodes.ServiceError.Timeout.Message,
                viesFault);
        }

        if (Equals(viesFault, "GLOBAL_MAX_CONCURRENT_REQ")
            || Equals(viesFault, "GLOBAL_MAX_CONCURRENT_REQ_TIME")
            || Equals(viesFault, "MS_MAX_CONCURRENT_REQ")
            || Equals(viesFault, "MS_MAX_CONCURRENT_REQ_TIME"))
        {
            return (
                ViesErrorCodes.ServiceError.RateLimitExceeded.Code,
                ViesErrorCodes.ServiceError.RateLimitExceeded.Message,
                viesFault);
        }

        return (viesFault, ViesErrorCodes.ServiceError.ServiceUnavailable.Message, viesFault);
    }

    private static bool Equals(string value, string other) =>
        string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
}
