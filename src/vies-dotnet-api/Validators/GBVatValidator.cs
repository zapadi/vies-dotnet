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

internal sealed class GbVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [8, 7, 6, 5, 4, 3, 2];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

         // Check for GD/HA prefixes
        if (vatSpan.Length == 5)
        {
            if (!vatSpan[2..].TryConvertToInt(out var no))
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"GD/HA: Last 3 characters must be digits");
            }

            if (vatSpan.StartsWith("GD", StringComparison.OrdinalIgnoreCase))
            {
                return no < 500
                    ? VatValidationResult.Success()
                    : VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid Government departments VAT");
            }

            if (vatSpan.StartsWith("HA", StringComparison.OrdinalIgnoreCase))
            {
                return no >= 500
                    ? VatValidationResult.Success()
                    : VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid Health authorities VAT");
            }
        }

        if (vatSpan.Length != 9)
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetLengthMessage(9));
        }

        // Check if the first digit not zero
        if (vatSpan[0] == '0')
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"0 VAT numbers disallowed");
        }

        // Parse first 7 digits for range check
        if (!vatSpan[..7].TryConvertToInt(out var first7digits))
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetInvalidFormatMessage());
        }

        var total = vatSpan.Sum(Multipliers);

        // Old numbers use a simple 97 modulus, but new numbers use an adaptation of that (less 55). Our

        // Establish check digits by subtracting 97 from total until negative.
        var cd = total;
        while (cd > 0)
        {
            cd -= 97;
        }

        // Get the absolute value and compare it with the last two characters of the VAT number. If the
        // same, then it is a valid traditional check digit. However, even then the number must fit within
        // certain specified ranges.
        cd = Math.Abs(cd);

        if (!vatSpan.Slice(7, 2).TryConvertToInt(out var checkDigits))
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetInvalidNumberMessage());
        }

        // Old method check
        if (cd == checkDigits && first7digits < 9990001 &&
            (first7digits < 100000 || first7digits > 999999) &&
            (first7digits < 9490001 || first7digits > 9700000))
        {
            return VatValidationResult.Success();
        }

        // New method check
        cd = cd >= 55 ? cd - 55 : cd + 42;

        return ValidateChecksumDigit(cd == checkDigits && first7digits > 1000000);
    }
}
