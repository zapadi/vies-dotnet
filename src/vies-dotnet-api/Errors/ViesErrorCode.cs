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
/// Error codes exposed by <see cref="VatValidationResult.ErrorCode"/> and <see cref="ViesException.ErrorCode"/>.
/// </summary>
public static class ViesErrorCode
{
    /// <summary>VAT number is registered but currently inactive.</summary>
    public const string VatInactive = "vat-inactive";

    /// <summary>VAT number could not be found in the VIES database.</summary>
    public const string VatNotFound = "vat-not-found";

    /// <summary>The VIES service is temporarily unavailable.</summary>
    public const string ServiceUnavailable = "service-unavailable";

    /// <summary>A network error occurred while contacting the VIES service.</summary>
    public const string NetworkError = "network-error";

    /// <summary>The request to the VIES service timed out.</summary>
    public const string Timeout = "timeout";

    /// <summary>The VIES service returned an invalid or unexpected response.</summary>
    public const string InvalidResponse = "invalid-response";

    /// <summary>The VIES service rate limit was exceeded.</summary>
    public const string RateLimitExceeded = "rate-limit-exceeded";

    /// <summary>The supplied input is invalid.</summary>
    public const string InvalidInput = "invalid-input";

    /// <summary>The VAT number is not in a valid format.</summary>
    public const string InvalidVatFormat = "invalid-vat-format";

    /// <summary>The VAT number has an invalid checksum digit.</summary>
    public const string InvalidVatChecksumDigit = "invalid-vat-checksum-digit";

    /// <summary>The country code is invalid or unsupported.</summary>
    public const string InvalidCountryCode = "invalid-country-code";

    /// <summary>The VAT number exceeds the maximum permitted length.</summary>
    public const string VatNumberTooLong = "vat-number-too-long";

    /// <summary>VAT validation is not available for the requested region.</summary>
    public const string RegionUnsupported = "region-unsupported";

    /// <summary>An unexpected error occurred.</summary>
    public const string UnexpectedError = "unexpected-error";

    /// <summary>Deserialization of the VIES response failed.</summary>
    public const string DeserializationFailed = "deserialization-failed";
}
