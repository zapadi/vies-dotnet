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
internal sealed class ElVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [256, 128, 64, 32, 16, 8, 4, 2];

    public ElVatValidator(string countryCode) : base(countryCode)
    {
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is not 8 and not 9)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthRangeMessage(8, 9));
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
        }

        var sum = 0;
        var controlValue = 0;
        if (vatSpan.Length == 8)
        {
            Span<char> paddedVat = stackalloc char[9];
            paddedVat[0] = '0';
            vatSpan.CopyTo(paddedVat[1..]);
            controlValue = paddedVat[8].ToInt();
            sum = paddedVat.Sum(Multipliers);
        }
        else
        {
            controlValue = vatSpan[8].ToInt();
            sum = vatSpan.Sum(Multipliers);
        }

        var checkDigit = sum % 11;

        if (checkDigit > 9)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(controlValue, checkDigit);
    }
}
