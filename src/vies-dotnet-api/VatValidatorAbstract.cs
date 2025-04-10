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

using Padi.Vies.Errors;

namespace Padi.Vies;

/// <summary>
///
/// </summary>
public abstract class VatValidatorAbstract : IVatValidator
{
    protected static string CountryCode { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="vat"></param>
    /// <returns></returns>
    /// <exception cref="ViesValidationException"></exception>
    public VatValidationResult Validate(string vat)
    {
        VatValidationResult result = OnValidate(vat);
        return result.IsValid
            ? result
            : VatValidationResult.Failed($"Invalid {CountryCode} VAT: format");
    }
    protected abstract VatValidationResult OnValidate(string vat);

    protected static VatValidationResult ValidateChecksumDigit(int digit, int checkDigit, string message = null)
    {
        var isValid = checkDigit == digit;
        return !isValid
            ? VatValidationResult.Failed(message ?? $"Invalid {CountryCode} VAT: checkValue")
            : VatValidationResult.Success();
    }

    protected static VatValidationResult ValidateChecksumDigit(bool isValid, string message = null)
    {
        return !isValid
            ? VatValidationResult.Failed(message ?? $"Invalid {CountryCode} VAT: checkValue")
            : VatValidationResult.Success();
    }
}
