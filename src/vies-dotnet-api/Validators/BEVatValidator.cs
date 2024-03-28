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
public sealed class BeVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN = @"^[0|1]?\d{9}$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.BE);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    public BeVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        if (vat.Length == 10 && vat[0] != '0' && vat[0] != '1')
        {
            return VatValidationResult.Failed("First character of 10 digit numbers should be 0 or 1.");
        }

        if (vat.Length == 9)
        {
            vat = vat.PadLeft(10, '0');
        }

        // Modulus 97 check on last nine digits
        var isValid = 97 - int.Parse(vat.Slice(0, 8), CultureInfo.InvariantCulture) % 97 == int.Parse(vat.Slice(8, 2), CultureInfo.InvariantCulture);

        return !isValid
            ? VatValidationResult.Failed("Invalid BE vat: checkValue.")
            : VatValidationResult.Success();
    }
}
