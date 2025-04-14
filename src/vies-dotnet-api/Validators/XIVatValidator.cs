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
using Padi.Vies.Extensions;
using Padi.Vies.Internal.Extensions;

namespace Padi.Vies.Validators;

internal sealed class XIVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [8, 7, 6, 5, 4, 3, 2];

    public XIVatValidator()
    {
        CountryCode = nameof(NonEuCountryCode.XI);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.StartsWith("GD".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            if (vatSpan.Length != 5)
            {
                return VatValidationResult.Failed($"Invalid length for {CountryCode} GD VAT number");
            }

            if (!vatSpan[2..].TryConvertToInt(out var no))
            {
                return VatValidationResult.Failed($"Invalid {CountryCode} for GD number format");
            }

            return no < 500
                ? VatValidationResult.Success()
                : VatValidationResult.Failed($"Invalid {CountryCode} VAT for Government departments");
        }

        if (vatSpan.StartsWith("HA".AsSpan(), StringComparison.OrdinalIgnoreCase) )
        {
            if (vatSpan.Length != 5)
            {
                return VatValidationResult.Failed($"Invalid length for {CountryCode} HA VAT number");
            }

            if (!vatSpan[2..].TryConvertToInt(out var no))
            {
                return VatValidationResult.Failed($"Invalid {CountryCode} HA VAT number format");
            }

            return no >= 500
                ? VatValidationResult.Success()
                : VatValidationResult.Failed($"Invalid {CountryCode} VAT for Health authorities");
        }

        if (vatSpan.Length is not 9 and not 12)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        var total = 0;
        for (var i = 0; i < 7; i++)
        {
            if (!char.IsDigit(vatSpan[i]))
            {
                return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
            }
            total += vatSpan[i].ToInt() * Multipliers[i];
        }

        if (!vatSpan.Slice(7, 2).TryConvertToInt(out var checkValue))
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: check digits");
        }

        total += checkValue;

        var result1 = total % 97;
        var result2 = (result1 + 55) % 97;

        var isValid = result1 == 0 || result2 == 0;
        return !isValid
            ? VatValidationResult.Failed($"Invalid {CountryCode} VAT: checkValue")
            : VatValidationResult.Success();
    }
}
