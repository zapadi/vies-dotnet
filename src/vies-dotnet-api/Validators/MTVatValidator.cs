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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators;

[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
public sealed class MtVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN =@"^[1-9]\d{7}$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.MT);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly int[] Multipliers = {3, 4, 6, 7, 8, 9};

    public MtVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        var sum = vat.Sum(Multipliers);

        var checkDigit = 37 - sum % 37;

        var isValid = checkDigit == int.Parse(vat.Slice(6, 2), NumberStyles.Integer, CultureInfo.InvariantCulture);

        return !isValid
            ? VatValidationResult.Failed("Invalid MT vat: checkValue")
            : VatValidationResult.Success();
    }
}
