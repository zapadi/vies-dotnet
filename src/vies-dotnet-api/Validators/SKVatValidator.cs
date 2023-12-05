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
public sealed class SkVatValidator : VatValidatorAbstract
{
    private const string REGEX_PATTERN = @"^[1-9]\d[2346-9]\d{7}$";
    private const string COUNTRY_CODE = nameof(EuCountryCode.SK);

    private static readonly Regex _regex = new(REGEX_PATTERN, RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    public SkVatValidator()
    {
        this.Regex = _regex;
        CountryCode = COUNTRY_CODE;
    }
        
    protected override VatValidationResult OnValidate(string vat)
    {
        var nr = ulong.Parse(vat, NumberStyles.Integer, CultureInfo.InvariantCulture);
        var isValid = nr % 11 == 0;
        return !isValid 
            ? VatValidationResult.Failed("Invalid SK vat: checkValue") 
            : VatValidationResult.Success();
    }
}