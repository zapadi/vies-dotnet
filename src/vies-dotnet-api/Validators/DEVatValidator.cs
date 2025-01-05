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
using Padi.Vies.Extensions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class DeVatValidator : VatValidatorAbstract
{
    public DeVatValidator()
    {
        CountryCode = nameof(EuCountryCode.DE);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 9)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if (vatSpan[0] is < '1' or > '9')
        {
            return VatValidationResult.Failed($"First digit must be 1-9 for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        var product = 10;
        for (var index = 0; index < 8; index++)
        {
            var sum = (vatSpan[index].ToInt() + product) % 10;
            if (sum == 0)
            {
                sum = 10;
            }

            product = 2 * sum % 11;
        }

        var val = 11 - product;
        var checkDigit = val == 10 ? 0 : val;

        return ValidateChecksumDigit(vatSpan[8].ToInt(), checkDigit);
    }
}
