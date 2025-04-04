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

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class SkVatValidator : VatValidatorAbstract
{
    public SkVatValidator()
    {
        CountryCode = nameof(EuCountryCode.SK);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 10)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if (vatSpan[0] is < '1' or > '9')
        {
            return VatValidationResult.Failed($"First digit must be 1-9 for {CountryCode} VAT number");
        }

        // Validate third digit (2,3,4,6,7,8,9) or is not (0,1,5)
        if (vatSpan[2].ToInt() is 0 or 1 or 5)
        {
            return VatValidationResult.Failed($"Third digit invalid for {CountryCode} VAT number");
        }

        if (!vatSpan.TryConvertToInt(out var nr))
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: parsing error");
        }

        return ValidateChecksumDigit(nr % 11, 0);
    }
}
