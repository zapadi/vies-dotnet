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
internal sealed class RoVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [7, 5, 3, 2, 1, 7, 5, 3, 2];

    public RoVatValidator()
    {
        CountryCode = nameof(EuCountryCode.RO);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is < 2 or > 10)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        var controlDigit = vatSpan[^1].ToInt();

        ReadOnlySpan<char> numberSpan = vatSpan[..^1];
        Span<char> paddedSpan = stackalloc char[9];
        var padding = 9 - numberSpan.Length;

       if (padding > 0)
        {
            paddedSpan[..padding].Fill('0');
            numberSpan.CopyTo(paddedSpan[padding..]);
        }
        else
        {
            numberSpan.CopyTo(paddedSpan);
        }

        var sum = paddedSpan.Sum(Multipliers);

        var checkDigit = sum * 10 % 11;

        if (checkDigit == 10)
        {
            checkDigit = 0;
        }

        return ValidateChecksumDigit(controlDigit, checkDigit);
    }
}
