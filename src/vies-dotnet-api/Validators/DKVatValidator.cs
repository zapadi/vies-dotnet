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
using Padi.Vies.Extensions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class DkVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [2, 7, 6, 5, 4, 3, 2, 1];

    public DkVatValidator()
    {
        CountryCode = nameof(EuCountryCode.DK);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length > 12)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        // Create buffer for sanitized number
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
                return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
            }

            cleanVat[cleanIndex++] = vatSpan[i];
        }

        if (cleanIndex != 8)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        var sum = cleanVat.Sum(Multipliers);

        return ValidateChecksumDigit(sum % 11, 0);
    }
}
