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
internal sealed class NlVatValidator : VatValidatorAbstract
{
    private static ReadOnlySpan<int> Multipliers => [9, 8, 7, 6, 5, 4, 3, 2];

    public NlVatValidator()
    {
        CountryCode = nameof(EuCountryCode.NL);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 12)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits(0, 9))
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: first 9 characters must be digits");
        }

        // Verify 'B' at position 10
        if (vatSpan[9] != 'B')
        {
            return VatValidationResult.Failed("Invalid NL VAT: position 10 must be 'B'");
        }

        // Verify last 2 chars are digits
        if (!char.IsDigit(vatSpan[10]) || !char.IsDigit(vatSpan[11]))
        {
            return VatValidationResult.Failed("Invalid NL VAT: last 2 characters must be digits");
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
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: parsing error");
        }

        return ValidateChecksumDigit((long)nr % 97 == 1);
    }
}
