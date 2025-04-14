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
internal sealed class BeVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is not 9 and not 10)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthRangeMessage(9, 10));
        }

        if (vatSpan.Length == 10 && vatSpan[0] is not '0' and not '1')
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, "For 10-digit numbers, first character must be '0' or '1'");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
        }

        int firstPart, checkPart;
        if (vatSpan.Length == 9)
        {
            if (!vatSpan[..7].TryConvertToInt(out firstPart) || !vatSpan[7..].TryConvertToInt(out checkPart))
            {
                return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetInvalidFormatMessage());
            }
        }
        else
        {
            if (!vatSpan[..8].TryConvertToInt(out firstPart) || !vatSpan[8..].TryConvertToInt(out checkPart))
            {
                return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
            }
        }

        // Modulus 97 check on last nine digits
        var modulus = 97 - firstPart % 97;

        return ValidateChecksumDigit(checkPart, modulus);
    }
}
