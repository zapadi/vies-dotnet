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

[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal sealed class XIVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN =@"^\d{9}|\d{12}|(GD|HA)\d{3}$";
    private const string COUNTRY_CODE = nameof(NonEuCountryCode.XI);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = {8, 7, 6, 5, 4, 3, 2};

    public XIVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        var prefix = vat.Slice(0, 2);
        if (string.Equals(prefix, "GD", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(vat.Slice(2, 3), NumberStyles.Integer, CultureInfo.InvariantCulture,out var no))
        {
            return no < 500
                ? VatValidationResult.Success()
                : VatValidationResult.Failed("Invalid Government departments VAT");
        }

        if (string.Equals(prefix, "HA", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(vat.Slice(2, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out no))
        {
            return no > 499
                ? VatValidationResult.Success()
                : VatValidationResult.Failed("Invalid Health authorities VAT");
        }

        var total = vat.Sum(Multipliers);
        total += int.Parse(vat.Slice(7, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);

        var result1 = total % 97;
        var result2 = (result1 + 55) % 97;

        var isValid = result1 ==0 || result2 == 0;
        return !isValid
            ? VatValidationResult.Failed($"Invalid {CountryCode} vat: checkValue")
            : VatValidationResult.Success();
    }
}
