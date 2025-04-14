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

internal sealed class ItVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [1, 2, 1, 2, 1, 2, 1, 2, 1, 2];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 11)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthMessage(11));
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
        }

        var allZeros = true;
        for (var i = 0; i < 7; i++)
        {
            if (vatSpan[i] == '0')
            {
                continue;
            }

            allZeros = false;
            break;
        }

        if (allZeros)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat,"First 7 digits cannot be all zeros");
        }

        // Validate office code (digits 8-10)
        if (!vatSpan.Slice(7, 3).TryConvertToInt(out var officeCode))
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat,"Invalid office code");
        }

        if (officeCode is < 1 or > 201 && officeCode != 999 && officeCode != 888)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat,"Invalid office code range");
        }

        var sum = 0;
        for (var i = 0; i < Multipliers.Length; i++)
        {
            var digit = vatSpan[i].ToInt();
            var product = digit * Multipliers[i];
            sum += product > 9
                ? (int) Math.Floor(product / 10D) + product % 10
                : product;
        }

        var checkDigit = 10 - sum % 10;

        if (checkDigit > 9)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vatSpan[10].ToInt(), checkDigit);
    }
}
