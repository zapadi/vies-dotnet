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

/// <summary>
///
/// </summary>
internal sealed class SiVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [8, 7, 6, 5, 4, 3, 2];

    public SiVatValidator()
    {
        CountryCode = nameof(EuCountryCode.SI);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 8)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if (vatSpan[0] is < '1' or > '9')
        {
            return VatValidationResult.Failed($"First digit must be 1-9 for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        var sum = vatSpan.Sum(Multipliers);

        var checkDigit = 11 - sum % 11;

        if (checkDigit > 9)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vat[7].ToInt(), checkDigit);
    }
}
