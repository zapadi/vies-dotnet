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

internal sealed class IeVatValidator(string countryCode) : VatValidatorAbstract(countryCode)
{
    private static ReadOnlySpan<int> Multipliers => [8, 7, 6, 5, 4, 3, 2];

    protected override VatValidationResult OnValidate(string vat)
    {
        ReadOnlySpan<char> vatSpan = vat.AsSpan();

        if (vatSpan.Length is <8 or >9)
        {
            return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetLengthRangeMessage(8, 9));
        }

        var multiplier = 0;
        Span<char> normalizedVat = stackalloc char[9];

        if (IsOldStyleFormat(vatSpan))
        {
            normalizedVat[0] = '0';
            vatSpan.Slice(2, 5).CopyTo(normalizedVat[1..]);
            normalizedVat[6] = vatSpan[0];
            normalizedVat[7] = vatSpan[7];
        }
        else
        {
            if (Is2013Format(vatSpan))
            {
                multiplier = vatSpan[8] == 'H'
                    ? 72
                    : vatSpan[8] == 'A'
                        ? 9
                        : 0;

                vatSpan.Slice(0, 7).CopyTo(normalizedVat);
                normalizedVat[7] = vatSpan[7];
            }
            else
            {
                if (IsPre2013Format(vatSpan))
                {
                    vatSpan.Slice(0, 7).CopyTo(normalizedVat);
                    normalizedVat[7] = vatSpan[7];
                }
                else
                {
                    return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetInvalidFormatMessage());
                }
            }
        }

        var sum = 0;
        for (var index = 0; index < 7; index++)
        {
            if (!char.IsDigit(normalizedVat[index]))
            {
                return VatValidationDispatcher.InvalidVatFormat(CountryCode, vat, VatValidationErrorMessageHelper.GetInvalidRangeDigitsMessage(1, 7));
            }

            sum += normalizedVat[index].ToInt() * Multipliers[index];
        }

        sum += multiplier;
        var checkDigit = sum % 23;
        var expectedCheck = checkDigit == 0 ? 'W' : (char)(64 + checkDigit);

        return ValidateChecksumDigit(normalizedVat[7], expectedCheck);
    }

    /// <summary>
    /// (1 digit + [letter/+/*] + 5 digits + letter)
    /// </summary>
    /// <param name="vat"></param>
    /// <returns></returns>
    private static bool IsOldStyleFormat(ReadOnlySpan<char> vat) =>
        vat.Length == 8
        && char.IsDigit(vat[0])
        && (char.IsLetter(vat[1]) || vat[1] is '+' or '*')
        && char.IsLetter(vat[7])
        && vat[2..7].ContainsOnlyDigits();

    /// <summary>
    /// 2013+ format (7 digits + letter + [AH])
    /// </summary>
    /// <param name="vat"></param>
    /// <returns></returns>
    private static bool Is2013Format(ReadOnlySpan<char> vat) =>
        vat.Length == 9
        && char.IsLetter(vat[7])
        && vat[..7].ContainsOnlyDigits()
        && (vat[8] is 'A' or 'H');

    /// <summary>
    /// pre-2013 format (7 digits)
    /// </summary>
    /// <param name="vat"></param>
    /// <returns></returns>
    private static bool IsPre2013Format(ReadOnlySpan<char> vat) =>
        vat.Length == 8
        && char.IsLetter(vat[7])
        && vat[..7].ContainsOnlyDigits();
}
