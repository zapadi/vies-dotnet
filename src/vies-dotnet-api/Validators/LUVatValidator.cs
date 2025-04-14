using System;
using Padi.Vies.Errors;
using Padi.Vies.Internal.Extensions;

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
using Padi.Vies.Extensions;

namespace Padi.Vies.Validators;

/// <summary>
///
/// </summary>
internal sealed class LuVatValidator : VatValidatorAbstract
{
    public LuVatValidator(string countryCode) : base(countryCode)
    {
    }

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length != 8)
        {
            return VatValidationResult.Failed($"Invalid length for {CountryCode} VAT number");
        }

        if(!vatSpan.ValidateAllDigits())
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: not all digits");
        }

        if (!vatSpan[..6].TryConvertToInt(out var baseNumber))
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: invalid base number");
        }

        if (!vatSpan[6..].TryConvertToInt(out var checkDigits))
        {
            return VatValidationResult.Failed($"Invalid {CountryCode} VAT: invalid check digits");
        }

        return ValidateChecksumDigit(baseNumber % 89 , checkDigits);
    }
}
