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
internal sealed class NlVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [9, 8, 7, 6, 5, 4, 3, 2];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 12)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthMessage(12));
        }

        if(!vatSpan.ValidateAllDigits(0, 9))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidRangeDigitsMessage(0, 9));
        }

        if (vatSpan[9] != 'B')
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidCharacterAtMessage(10, "'B'"));
        }

        if (!char.IsDigit(vatSpan[10]) || !char.IsDigit(vatSpan[11]))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidNumberMessage());
        }

        var sum = vatSpan.Sum(Multipliers);

        // Old VAT numbers (pre-2020) - Modulus 11 test
        var checkMod11 = sum % 11;
        if (checkMod11 > 9)
        {
            checkMod11 = 0;
        }

        var isValidMod11 = checkMod11 == vat[8].ToInt();
        if (isValidMod11)
        {
            return VatValidationResult.Success();
        }

        // New VAT numbers (post 2020) - Modulus 97 test
        Span<char> mod97Input = stackalloc char[17];

        ReadOnlySpan<char> nlValue = ['2','3','2','1'];
        ReadOnlySpan<char> bValueMap = ['1','1'];

        nlValue.CopyTo(mod97Input);
        vatSpan[..9].CopyTo(mod97Input[4..]);

        bValueMap.CopyTo(mod97Input[13..]);
        vatSpan[10..].CopyTo(mod97Input[15..]);

        if (!mod97Input.TryConvertToLong(out var nr))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat,VatValidationErrorMessageHelper.GetInvalidFormatMessage());
        }

        return ValidateChecksumDigit((long)nr % 97 == 1);
    }
}
