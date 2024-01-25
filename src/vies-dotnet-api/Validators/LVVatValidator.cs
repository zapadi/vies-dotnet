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
public sealed class LvVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN =@"^\d{11}$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.LV);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = {9, 1, 4, 8, 3, 10, 2, 5, 7, 6};

    public LvVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        // Only check the legal bodies
        if (Regex.IsMatch(vat, "/^[0-3]/", RegexOptions.None, TimeSpan.FromSeconds(5)))
        {
            var result = Regex.IsMatch(vat, "^[0-3][0-9][0-1][0-9]", RegexOptions.None, TimeSpan.FromSeconds(5));
            return !result
                ? VatValidationResult.Failed("Invalid LV vat: checkValue")
                : VatValidationResult.Success();
        }
        var sum = vat.Sum(Multipliers);

        var checkDigit = sum % 11;

        if (checkDigit == 4 && vat[0] == '9')
        {
            checkDigit -= 45;
        }

        if (checkDigit == 4)
        {
            checkDigit = 4 - checkDigit;
        }
        else
        {
            if (checkDigit > 4)
            {
                checkDigit = 14 - checkDigit;
            }
            else
            {
                checkDigit = 3 - checkDigit;
            }
        }

        var isValid = checkDigit == vat[10].ToInt();

        return !isValid
            ? VatValidationResult.Failed("Invalid LV vat: checkValue")
            : VatValidationResult.Success();
    }
}
