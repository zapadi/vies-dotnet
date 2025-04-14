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
internal sealed class LvVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [9, 1, 4, 8, 3, 10, 2, 5, 7, 6];

    public LvVatValidator(string countryCode) : base(countryCode)
    {
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 11)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthMessage(11));
        }

        if (vatSpan[0] == '0')
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat,VatValidationErrorMessageHelper.GetInvalidCharacterAtMessage(0, "different than '0'"));
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
        }

        // Only check the legal bodies
        if(vatSpan[0] is >= '0' and <= '3')
        {
            if(vatSpan[1] is >= '0' and <= '9' && vatSpan[2] is >= '0' and <= '1' && vatSpan[3] is >= '0' and <= '9')
            {
                return VatValidationResult.Success();
            }

            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat,VatValidationErrorMessageHelper.GetInvalidChecksumMessage());
        }

        var sum = vatSpan.Sum(Multipliers);

        var checkDigit = sum % 11;

        if (checkDigit == 4 && vatSpan[0] == '9')
        {
            checkDigit -= 45;
        }

        checkDigit = checkDigit switch
        {
            4 => 4 - checkDigit,
            > 4 => 14 - checkDigit,
            _ => 3 - checkDigit,
        };

        return ValidateChecksumDigit(vatSpan[10].ToInt(), checkDigit);
    }
}
