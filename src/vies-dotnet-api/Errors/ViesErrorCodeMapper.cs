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

internal static class ViesErrorCodeMapper
{
    private static class ViesFault
    {
        public const string InvalidInput = "INVALID_INPUT";
        public const string InvalidRequesterInfo = "INVALID_REQUESTER_INFO";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
        public const string MsUnavailable = "MS_UNAVAILABLE";
        public const string Timeout = "TIMEOUT";
        public const string GlobalMaxConcurrentReq = "GLOBAL_MAX_CONCURRENT_REQ";
        public const string GlobalMaxConcurrentReqTime = "GLOBAL_MAX_CONCURRENT_REQ_TIME";
        public const string MsMaxConcurrentReq = "MS_MAX_CONCURRENT_REQ";
        public const string MsMaxConcurrentReqTime = "MS_MAX_CONCURRENT_REQ_TIME";
    }

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
    /// Any unrecognized fault maps to the "service-unavailable" error code;
    /// the raw fault text is preserved as the user message.
    /// </summary>
    public static (string Code, string Message, string UserMessage) Map(string? viesFault)
    {
        if (string.IsNullOrWhiteSpace(viesFault))
        {
            return (
                ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
                ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
                ViesErrorCodes.ServiceError.ServiceUnavailable.UserMessage);
        }

        return viesFault!.ToUpperInvariant() switch
        {
            ViesFault.InvalidInput or ViesFault.InvalidRequesterInfo => (
                ViesErrorCodes.InputError.InvalidInput.Code,
                ViesErrorCodes.InputError.InvalidInput.Message,
                viesFault),
            ViesFault.ServiceUnavailable or ViesFault.MsUnavailable => (
                ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
                ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
                viesFault),
            ViesFault.Timeout => (
                ViesErrorCodes.ServiceError.Timeout.Code,
                ViesErrorCodes.ServiceError.Timeout.Message,
                viesFault),
            ViesFault.GlobalMaxConcurrentReq
                or ViesFault.GlobalMaxConcurrentReqTime
                or ViesFault.MsMaxConcurrentReq
                or ViesFault.MsMaxConcurrentReqTime => (
                ViesErrorCodes.ServiceError.RateLimitExceeded.Code,
                ViesErrorCodes.ServiceError.RateLimitExceeded.Message,
                viesFault),
            _ => (
                ViesErrorCodes.ServiceError.ServiceUnavailable.Code,
                ViesErrorCodes.ServiceError.ServiceUnavailable.Message,
                viesFault),
        };
    }
}
