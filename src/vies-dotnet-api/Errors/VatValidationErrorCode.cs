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

/// <summary>
/// Contains all VAT validation error codes
/// </summary>
internal static class VatValidationErrorCode
{
    public const string InvalidVat = "INVALID_VAT";
    public const string InvalidEUVat = "INVALID_EU_VAT";
    public const string InvalidLength = "INVALID_LENGTH";
    public const string InvalidFormat = "INVALID_FORMAT";
    public const string InvalidCheckDigit = "INVALID_CHECK_DIGIT";
}
