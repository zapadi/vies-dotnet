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
/// Error categories exposed by <see cref="ViesException.ErrorType"/>.
/// </summary>
public static class ViesErrorType
{
    /// <summary>The request was invalid (for example, an inactive or unknown VAT number).</summary>
    public const string InvalidRequest = "invalid_request_error";

    /// <summary>The VIES service is unavailable, timed out or otherwise failed.</summary>
    public const string Service = "service_error";

    /// <summary>The supplied input is invalid.</summary>
    public const string Input = "input_error";

    /// <summary>A validation rule (format, checksum, country code, length) failed.</summary>
    public const string Validation = "validation_error";

    /// <summary>VAT validation is not supported for the requested region.</summary>
    public const string UnsupportedRegion = "unsupported_region_error";

    /// <summary>An unexpected internal error occurred.</summary>
    public const string Api = "api_error";

    /// <summary>Deserialization of the VIES response failed.</summary>
    public const string Deserialization = "deserialization_error";
}
