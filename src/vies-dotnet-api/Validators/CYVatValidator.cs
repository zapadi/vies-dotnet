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
internal sealed class CyVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is < 9 or > 10)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthRangeMessage(9, 10));
        }

        if (!vatSpan.ValidateAllDigits(0, 8))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidRangeDigitsMessage(0, 8));
        }

        if (vatSpan[..2] is "12")
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidPrefixMessage(2, "different than 12"));
        }

        if (!char.IsLetter(vatSpan[8]))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidCharacterAtMessage(8, "letter"));
        }

        var result = 0;
        for (var index = 0; index < 8; index++)
        {
            var temp = vatSpan[index].ToInt();

            if (index % 2 == 0)
            {
                temp = temp switch
                {
                    0 => 1,
                    1 => 0,
                    2 => 5,
                    3 => 7,
                    4 => 9,
                    _ => temp * 2 + 3,
                };
            }
            result += temp;
        }

        var checkDigit = result % 26 + 65;

        return ValidateChecksumDigit(vatSpan[8], checkDigit);
    }
}
