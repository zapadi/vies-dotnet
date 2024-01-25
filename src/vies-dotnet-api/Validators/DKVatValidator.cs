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
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
public sealed class DkVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN = @"^(\d{2} ?){3}\d{2}$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.DK);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = {2, 7, 6, 5, 4, 3, 2, 1};

    public DkVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        var sum = vat.Sum(Multipliers);

        var isValid = sum % 11 == 0;
        return !isValid
            ? VatValidationResult.Failed("Invalid DK vat: checkValue")
            : VatValidationResult.Success();
    }
}
