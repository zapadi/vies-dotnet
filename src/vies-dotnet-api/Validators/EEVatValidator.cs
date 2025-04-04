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
internal sealed class EeVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [3, 7, 1, 3, 7, 1, 3, 7];

    public EeVatValidator()
    {
        CountryCode = nameof(EuCountryCode.EE);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 9)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if (vatSpan[..2] is not "10")
        {
            return VatValidationResult.Failed($"Invalid prefix for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        var sum = vatSpan.Sum(Multipliers);

        var checkDigit = 10 - sum % 10;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vatSpan[8].ToInt(), checkDigit);
    }
}
