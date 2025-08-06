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
internal sealed class PlVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [6, 5, 7, 2, 3, 4, 5, 6, 7];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 10)
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetLengthMessage(10));
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
        }

        var sum = vatSpan.Sum(Multipliers);

        var checkDigit = sum % 11;

        if (checkDigit > 9)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vatSpan[9].ToInt(), checkDigit);
    }
}
