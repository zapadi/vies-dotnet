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

/// <summary>
///
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
public sealed class FrVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN =@"[0-9A-Z]{2}[0-9]{9}";
    private const string COUNTRY_CODE = nameof(EuCountryCode.FR);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    public FrVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        var validationKey = vat.Slice(0, 2);

        if (!int.TryParse(vat.Slice(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var val))
        {
            return VatValidationResult.Failed("Invalid FR vat: too long");
        }

        if (!int.TryParse(validationKey, NumberStyles.Integer, CultureInfo.InvariantCulture, out var temp))
        {
            return VatValidationResult.Success();
        }

        var checkDigit = ( 12 + 3 * ( val % 97 ) ) % 97;

        var isValid = checkDigit == temp;
        return !isValid
            ? VatValidationResult.Failed("Invalid FR vat: checkValue")
            : VatValidationResult.Success();
    }
}
