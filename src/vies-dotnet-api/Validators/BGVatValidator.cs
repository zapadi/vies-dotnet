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
internal sealed class BgVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> MultipliersPhysicalPerson => [2, 4, 8, 5, 10, 9, 7, 3, 6];
    private static ReadOnlySpan<int> MultipliersForeignPhysicalPerson => [21, 19, 17, 13, 11, 9, 7, 3, 1];
    private static ReadOnlySpan<int> MultipliersMiscellaneous => [4, 3, 2, 7, 6, 5, 4, 3, 2];

    public BgVatValidator()
    {
        CountryCode = nameof(EuCountryCode.BG);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is not 9 and not 10)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        if (vatSpan.Length == 9)
        {
            return Validate9DigitVat(vatSpan);
        }

        return ValidatePhysicalPerson(vatSpan) ??
               ValidateForeignPerson(vatSpan) ??
               ValidateMiscellaneous(vatSpan) ??
            VatValidationResult.Failed($"Invalid {CountryCode} VAT number");
    }

    private static VatValidationResult Validate9DigitVat(ReadOnlySpan<char> vatSpan)
    {
        var sum = vatSpan.Sum(MultipliersPhysicalPerson);

        var checkDigit = sum % 11;
        if (checkDigit == 10)
        {
            sum = 0;
            for (var index = 0; index < 8; index++)
            {
                sum += vatSpan[index].ToInt() * (index + 3);
            }
            checkDigit = sum % 11;
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }
        }

        return ValidateChecksumDigit(vatSpan[8].ToInt(), checkDigit, $"Invalid checksum for 9-digit {CountryCode} VAT");
    }

    private static VatValidationResult ValidatePhysicalPerson(ReadOnlySpan<char> vatSpan)
    {
        // Check date format (YYMMDD)
        vatSpan.Slice(2, 2).TryConvertToInt(out var month);
        vatSpan.Slice(4, 2).TryConvertToInt(out var day);

        if (month is < 1 or > 12 || day is < 1 or > 31)
        {
            return null;
        }

        var sum = vatSpan.Sum(MultipliersPhysicalPerson);

        var checkDigit = sum % 11;
        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return checkDigit == vatSpan[9].ToInt()
            ? VatValidationResult.Success()
            : null;
    }

    private static VatValidationResult ValidateForeignPerson(ReadOnlySpan<char> vatSpan)
    {
        var sum = vatSpan.Sum(MultipliersForeignPhysicalPerson);

        var checkDigit = sum % 10;
        return checkDigit == vatSpan[9].ToInt()
            ? VatValidationResult.Success()
            : null;
    }

    private static VatValidationResult ValidateMiscellaneous(ReadOnlySpan<char> vatSpan)
    {
        var sum = vatSpan.Sum(MultipliersMiscellaneous);

        var checkDigit = sum % 11;
        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return checkDigit == vatSpan[9].ToInt()
            ? VatValidationResult.Success()
            : null;
    }
}
