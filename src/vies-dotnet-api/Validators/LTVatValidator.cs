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
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
public sealed class LtVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN = @"^(\d{9}|\d{12})$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.LT);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = { 3, 4, 5, 6, 7, 8, 9, 1 };
    private static readonly int[] MultipliersTemporarily = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 2 };
    private static readonly int[] MultipliersDoubleCheck = { 3, 4, 5, 6, 7, 8, 9, 1, 2, 3, 4 };

    public LtVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        if (vat.Length != 9)
        {
            return TemporarilyRegisteredTaxPayers(vat);
        }

        if (vat[7] != '1')
        {
            return VatValidationResult.Failed("9 character VAT numbers should have 1 in 8th position.");
        }

        var sum = 0;
        for (var index = 0; index < 8; index++)
        {
            sum += vat[index].ToInt() * (index + 1);
        }

        if (sum % 11 == 10)
        {
            // Double check calculation
            sum = vat.Sum(Multipliers);
        }
        var checkDigit = sum % 11;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        var isValid = checkDigit == vat[8].ToInt();

        return !isValid
            ? VatValidationResult.Failed("Invalid LT vat: checkValue")
            : VatValidationResult.Success();
    }

    private static VatValidationResult TemporarilyRegisteredTaxPayers(string vat)
    {
        if (vat[10] != '1')
        {
            return VatValidationResult.Failed("Temporarily Registered Tax Payers should have 11th character one");
        }

        var total = vat.Sum(MultipliersTemporarily);

        // double check digit calculation
        if (total % 11 == 10)
        {
            total = vat.Sum(MultipliersDoubleCheck);
        }

        // Establish check digit.
        total %= 11;
        if (total == 10)
        {
            total = 0;
        }

        var isValid = total == vat[11].ToInt();
        return !isValid
            ? VatValidationResult.Failed("Invalid LT vat: checkValue")
            : VatValidationResult.Success();
    }
}
