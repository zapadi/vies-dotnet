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

namespace Padi.Vies;

/// <summary>
/// Represents the result of a VAT number validation operation
/// </summary>
public sealed class VatValidationResult
{
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <returns>A validation result indicating success</returns>
    public static VatValidationResult Success()
    {
        return new VatValidationResult
        {
            IsValid = true,
        };
    }

    /// <summary>
    /// Creates a failed validation result with the specified error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A validation result indicating failure</returns>
    [Obsolete("Use Failed(string countryCode, string errorCode, string errorMessage) instead.")]
    public static VatValidationResult Failed(string errorMessage)
    {
        return new VatValidationResult
        {
            Error = errorMessage,
        };
    }

    /// <summary>
    /// Creates a failed validation result with the specified country code, error code, and error message
    /// </summary>
    /// <param name="countryCode">The country code</param>
    /// <param name="errorCode">The error code</param>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A validation result indicating failure</returns>
    public static VatValidationResult Failed(string countryCode, string errorCode, string errorMessage, string param = null, string userMessage = null)
    {
        return new VatValidationResult
        {
            Error = errorMessage,
            ErrorCode = errorCode,
            CountryCode = countryCode,
            Param = param,
            UserMessage = userMessage,
        };
    }

    /// <summary>
    /// Gets a value indicating whether the validation was successful
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the error message if the validation failed
    /// </summary>
    public string Error { get; private set; }

    /// <summary>
    /// Gets the error code if the validation failed
    /// </summary>
    public string ErrorCode { get; private set; }

    /// <summary>
    /// Gets the country code for which the validation was performed
    /// </summary>
    public string CountryCode { get; private set; }

    /// <summary>
    /// Indicates the specific field causing the error
    /// </summary>
    public string Param { get; private set; }

    /// <summary>
    ///
    /// </summary>
    public string UserMessage { get; private set; }
}
