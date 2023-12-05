/*
   Copyright 2017-2023 Adrian Popescu.
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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators;

/// <summary>
/// 
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
public sealed class NlVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN = @"^\d{9}B\d{2}$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.NL);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = { 9, 8, 7, 6, 5, 4, 3, 2 };

    public NlVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        var sum = vat.Sum(Multipliers);

        // Old VAT numbers (pre 2020) - Modulus 11 test
        var checkMod11 = sum % 11;
        if (checkMod11 > 9)
        {
            checkMod11 = 0;
        }
        var isValidMod11 = checkMod11 == vat[8].ToInt();
        if (isValidMod11)
        {
            return VatValidationResult.Success();
        }
            
        // New VAT numbers (post 2020) - Modulus 97 test
        const string stringValueNl = "2321";
        const string stringValueB = "11";
        vat = vat.ReplaceString("B", stringValueB);

        var isValidMod97 = long.Parse($"{stringValueNl}{vat}", NumberStyles.Integer, CultureInfo.InvariantCulture) % 97 == 1;

        return !isValidMod97
            ? VatValidationResult.Failed("Invalid NL vat: checkValue")
            : VatValidationResult.Success();
    }
}