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
using Padi.Vies.Errors;
using Padi.Vies.Internal.Extensions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class AtVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [1, 2, 1, 2, 1, 2, 1];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 9)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthMessage(9));
        }

        if (vatSpan[0] != 'U')
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidCharacterAtMessage(0, "U"));
        }

        if(!vatSpan.ValidateAllDigits(1))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidRangeDigitsMessage(1, 8));
        }

        var sum = 0;
        var index = 1;
        foreach (var digit in Multipliers)
        {
            var product = vatSpan[index++].ToInt() * digit;
            sum += product > 9
                ? (int) Math.Floor(product / 10D) + product % 10
                : product;
        }

        var checkDigit = 10 - (sum + 4) % 10;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vatSpan[8].ToInt(), checkDigit);
    }
}
