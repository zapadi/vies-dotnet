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
using Padi.Vies.Internal.Extensions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class EsVatValidator : VatValidatorAbstract
{
    public EsVatValidator()
    {
        CountryCode = nameof(EuCountryCode.ES);
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        var length = vatSpan.Length;

        if (length != 9)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        var firstChar = vatSpan[0];
        var lastChar = vatSpan[^1];

        ReadOnlySpan<char> middleDigits = vatSpan.Slice(1, 7);

        if(!middleDigits.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        // Pattern 1: Letter + 8 digits
        if (char.IsLetter(firstChar) && char.IsDigit(lastChar))
        {
            return VatValidationResult.Success();
        }

        // Pattern 2: [A-HN-SW] + 7 digits + [A-J]
        if (firstChar is >= 'A' and <= 'H' or >= 'N' and <= 'S' or 'W' && lastChar is >= 'A' and <= 'J')
        {
            return VatValidationResult.Success();
        }

        // Pattern 3: [0-9YZ] + 7 digits + letter
        if ((char.IsDigit(firstChar) || firstChar is 'Y' or 'Z') && char.IsLetter(lastChar))
        {
            return VatValidationResult.Success();
        }

        // Pattern 4: [KLMX] + 7 digits + letter
        if (firstChar is 'K' or 'L' or 'M' or 'X' && char.IsLetter(lastChar))
        {
            return VatValidationResult.Success();
        }

        return VatValidationResult.Failed($"Invalid {CountryCode} VAT format");
    }
}
