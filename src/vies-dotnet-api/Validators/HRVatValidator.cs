/*
   Copyright 2017-2025 Adrian Popescu.
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

internal sealed class HrVatValidator : VatValidatorAbstract
{
    public HrVatValidator()
    {
        CountryCode = nameof(EuCountryCode.HR);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 11)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        var product = 10;

        for (var index = 0; index < 10; index++)
        {
            var sum = (vatSpan[index].ToInt() + product) % 10;

            if (sum == 0)
            {
                sum = 10;
            }

            product = 2 * sum % 11;
        }

        var checkDigit = (product + vatSpan[10].ToInt()) % 10;

        return ValidateChecksumDigit(1, checkDigit);
    }
}
