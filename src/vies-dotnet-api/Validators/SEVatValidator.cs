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
internal sealed class SeVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [2, 1, 2, 1, 2, 1, 2, 1, 2];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 12)
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetLengthMessage(12));
        }

        if (vatSpan[^2..] is not "01")
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat,"Last 2 characters must be '0' and '1'");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetAllDigitsMessage());
        }

        var sum = 0;

        for (var index = 0; index < Multipliers.Length; index++)
        {
            var digit = vatSpan[index].ToInt();
            var product = digit * Multipliers[index];
            sum += product > 9 ? (int) Math.Floor(product / 10D) + product % 10 : product;
        }

        var checkDigit = 10 - sum % 10;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(vatSpan[9].ToInt(), checkDigit);
    }
}
