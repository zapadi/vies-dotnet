/*
   Copyright 2017-2024 Adrian Popescu.
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
internal sealed class LtVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [3, 4, 5, 6, 7, 8, 9, 1 ];
    private static ReadOnlySpan<int> MultipliersTemporarily => [1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 2];
    private static ReadOnlySpan<int> MultipliersDoubleCheck => [3, 4, 5, 6, 7, 8, 9, 1, 2, 3, 4];

    public LtVatValidator()
    {
        CountryCode = nameof(EuCountryCode.LT);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is not 9 and not 12)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        return vatSpan.Length == 9
            ? ValidateNineDigitVat(vatSpan)
            : ValidateTemporaryVat(vatSpan);
    }

    private static VatValidationResult ValidateNineDigitVat(ReadOnlySpan<char> vatSpan)
    {
        if (vatSpan[7] != '1')
        {
            return VatValidationResult.Failed($"9 character {CountryCode} VAT numbers should have 1 in 8th position.");
        }

        var sum = 0;
        for (var index = 0; index < 8; index++)
        {
            sum += vatSpan[index].ToInt() * (index + 1);
        }

        if (sum % 11 == 10)
        {
            // Double check calculation
             sum = vatSpan.Sum(Multipliers);
        }

        var checkDigit = sum % 11;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vatSpan[8].ToInt(), checkDigit);
    }

    private static VatValidationResult ValidateTemporaryVat(ReadOnlySpan<char> vatSpan)
    {
        if (vatSpan[10] != '1')
        {
            return VatValidationResult.Failed("Temporarily Registered Tax Payers should have 11th character one");
        }

        var total = vatSpan.Sum(MultipliersTemporarily);

         if (total % 11 == 10)
        {
            total = vatSpan.Sum(MultipliersDoubleCheck);
        }

        total %= 11;
        if (total == 10)
        {
            total = 0;
        }

        return ValidateChecksumDigit(vatSpan[11].ToInt(), total);
    }
}
