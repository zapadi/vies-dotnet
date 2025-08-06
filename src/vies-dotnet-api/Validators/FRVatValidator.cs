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
using Padi.Vies.Errors;
using Padi.Vies.Internal.Extensions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class FrVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 11)
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetLengthMessage(11));
        }

        ReadOnlySpan<char> validationKey = vatSpan[..2];
        ReadOnlySpan<char> numericPart = vatSpan[2..];

        if(!numericPart.ValidateAllDigits())
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, "Characters from position 3 onwards must be digits");
        }

        if (!numericPart.TryConvertToInt(out var numericValue))
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, "Invalid numeric format for characters from position 3 onwards");
        }

        // If key is not numeric, consider valid
        if (!validationKey.TryConvertToInt(out var keyValue))
        {
            return VatValidationResult.Success();
        }

        var checkDigit = (12 + 3 * (numericValue % 97)) % 97;

        return ValidateChecksumDigit(keyValue, checkDigit);
    }
}
