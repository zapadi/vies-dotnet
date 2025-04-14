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
internal sealed class DkVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [2, 7, 6, 5, 4, 3, 2, 1];

    public DkVatValidator(string countryCode) : base(countryCode)
    {
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length > 12)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidLength, VatValidationErrorMessageHelper.GetLengthExceedMessage(12));
        }

        Span<char> cleanVat = stackalloc char[8];
        var cleanIndex = 0;

        for (var i = 0; i < vatSpan.Length && cleanIndex < 8; i++)
        {
            if (char.IsWhiteSpace(vatSpan[i]))
            {
                continue;
            }

            if (!char.IsDigit(vatSpan[i]))
            {
                return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, "All non-whitespace characters must be digits");
            }

            cleanVat[cleanIndex] = vatSpan[i];
            cleanIndex += 1;
        }

        if (cleanIndex != 8)
        {
            return VatValidationResult.Failed(CountryCode, VatValidationErrorCode.InvalidFormat, "Must contain exactly 8 digits (excluding whitespace)");
        }

        var sum = cleanVat.Sum(Multipliers);

        return ValidateChecksumDigit(sum % 11, 0);
    }
}
