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

namespace Padi.Vies.Errors;

internal static class ViesErrorCodes
{
    public static class InvalidRequestError
    {
        public const string Type = "invalid_request_error";

        public static class VatInactive
        {
            public const string Code = "vat-inactive";
            public const string Message = "VAT number is registered but inactive.";
            public const string UserMessage = "The VAT number is not currently active.";
            public const string Param = "vatNumber";
        }

        public static class VatNotFound
        {
            public const string Code = "vat-not-found";
            public const string Message = "VAT number not found in VIES database.";
            public const string UserMessage = "The VAT number could not be found.";
            public const string Param = "vatNumber";
        }
    }

    public static class ServiceError
    {
        public const string Type = "service_error";

        public static class ServiceUnavailable
        {
            public const string Code = "service-unavailable";
            public const string Message = "VIES service is temporarily unavailable.";
            public const string UserMessage = "The VAT validation service is currently down.";
        }

        public static class NetworkError
        {
            public const string Code = "network-error";
            public const string Message = "Network error occurred while contacting VIES.";
            public const string UserMessage = "Unable to connect to the validation service.";
        }

        public static class Timeout
        {
            public const string Code = "timeout";
            public const string Message = "Request to VIES service timed out.";
            public const string UserMessage = "The validation request took too long.";
        }

        public static class InvalidResponse
        {
            public const string Code = "invalid-response";
            public const string Message = "Received invalid response from VIES service.";
            public const string UserMessage = "The validation service returned an error.";
        }

        public static class RateLimitExceeded
        {
            public const string Code = "rate-limit-exceeded";
            public const string Message = "VIES service rate limit exceeded.";
            public const string UserMessage = "Too many requests. Please try again later.";
        }
    }

    public static class InputError
    {
        public const string Type = "input_error";

        public static class InvalidInput
        {
            public const string Code = "invalid-input";
            public const string Message = "The input is invalid.";
        }
    }

    public static class ValidationError
    {
        public const string Type = "validation_error";

        public static class InvalidVatFormat
        {
            public const string Code = "invalid-vat-format";
            public const string Message = "Invalid VAT number format.";
            public const string UserMessage = "The VAT number entered is not in a valid format.";
            public const string Param = "vatNumber";
        }

        public static class InvalidVatChecksumDigit
        {
            public const string Code = "invalid-vat-checksum-digit";
            public const string Message = "Invalid VAT checksum digit.";
            public const string UserMessage = "The VAT number entered has an invalid checksum digit.";
        }

        public static class InvalidCountryCode
        {
            public const string Code = "invalid-country-code";
            public const string Message = "Invalid or unsupported country code.";
            public const string UserMessage = "The country code is not valid.";
            public const string Param = "countryCode";
        }

        public static class VatNumberTooLong
        {
            public const string Code = "vat-number-too-long";
            public const string Message = "VAT number exceeds maximum length.";
            public const string UserMessage = "The VAT number is too long.";
            public const string Param = "vatNumber";
        }
    }

    public static class UnsupportedRegionError
    {
        public const string Type = "unsupported_region_error";

        public static class RegionUnsupported
        {
            public const string Code = "region-unsupported";
            public const string Message = "VAT validation is not available for this region.";
            public const string Param = "countryCode";
        }
    }

    public static class ApiError
    {
        public const string Type = "api_error";

        public static class UnexpectedError
        {
            public const string Code = "unexpected-error";
            public const string Message = "An unexpected error occurred.";
            public const string UserMessage = "Something went wrong. Please try again.";
        }
    }

    public static class DeserializationError
    {
        public const string Type = "deserialization_error";

        public static class Failed
        {
            public const string Code = "deserialization-failed";
            public const string Message = "Deserialization failed.";
            public const string UserMessage = "Could not deserialize the response from VIES service.";
        }
    }
}
