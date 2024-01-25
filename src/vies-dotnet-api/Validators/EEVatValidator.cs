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
public sealed class EeVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN =@"^10\d{7}$"; //[0-9]{9}
    private const string COUNTRY_CODE = nameof(EuCountryCode.EE);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = {3, 7, 1, 3, 7, 1, 3, 7};

    public EeVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }


    protected override VatValidationResult OnValidate(string vat)
    {
        var sum = vat.Sum(Multipliers);

        var checkDigit = 10 - sum % 10;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        var isValid = checkDigit == vat[8].ToInt();
        return !isValid
            ? VatValidationResult.Failed("Invalid EE vat: checkValue")
            : VatValidationResult.Success();
    }
}
