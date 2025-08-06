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
internal sealed class XiVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [8, 7, 6, 5, 4, 3, 2];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.StartsWith("GD".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            if (vatSpan.Length != 5)
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"GD code length must be 5 characters");
            }

            if (!vatSpan[2..].TryConvertToInt(out var no))
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid GD number format");
            }

            return no < 500
                ? VatValidationResult.Success()
                : VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid for Government departments");
        }

        if (vatSpan.StartsWith("HA".AsSpan(), StringComparison.OrdinalIgnoreCase) )
        {
            if (vatSpan.Length != 5)
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"HA code length must be 5 characters");
            }

            if (!vatSpan[2..].TryConvertToInt(out var no))
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid HA number format");
            }

            return no >= 500
                ? VatValidationResult.Success()
                : VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid for Health authorities");
        }

        if (vatSpan.Length is not 9 and not 12)
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Length must not be 9 or 12 characters");
        }

        var total = 0;
        for (var i = 0; i < 7; i++)
        {
            if (!char.IsDigit(vatSpan[i]))
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"All characters must be digits");
            }
            total += vatSpan[i].ToInt() * Multipliers[i];
        }

        if (!vatSpan.Slice(7, 2).TryConvertToInt(out var checkValue))
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Invalid format for last 2 digits");
        }

        total += checkValue;

        var result1 = total % 97;
        var result2 = (result1 + 55) % 97;

        return ValidateChecksumDigit(result1 == 0 || result2 == 0);
    }
}
