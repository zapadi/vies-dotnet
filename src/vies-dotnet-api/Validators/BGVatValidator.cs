/*
   Copyright 2017-2022 Adrian Popescu.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the spevatic language governing permissions and
   limitations under the License.
*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BGVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^\d{9,10}$";
        private static readonly Regex RegexPhysicalPerson = new Regex(@"^\d\d[0-5]\d[0-3]\d\d{4}$", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        private static readonly int[] MultipliersPhysicalPerson = {2, 4, 8, 5, 10, 9, 7, 3, 6};
        private static readonly int[] MultipliersForeignPhysicalPerson = {21, 19, 17, 13, 11, 9, 7, 3, 1};
        private static readonly int[] MultipliersMiscellaneous = {4, 3, 2, 7, 6, 5, 4, 3, 2};

        public BGVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(5));    
            CountryCode = nameof(EuCountryCode.BG);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            bool isValid;
            if (vat.Length == 9)
            {
                isValid = Bg9DigitsVat(vat);
                return isValid ? VatValidationResult.Success() : VatValidationResult.Failed("Invalid 9 digits vat.");
            }

            if (BgPhysicalPerson(vat))
            {
                return VatValidationResult.Success();
            }

            isValid = BgForeignerPhysicalPerson(vat) || BgMiscellaneousVatNumber(vat);

            return !isValid
                ? VatValidationResult.Failed("")
                : VatValidationResult.Success();
        }

        private static bool Bg9DigitsVat(string vat)
        {
            var total = 0;
            var temp = 0;
            for (var index = 0; index < 8; index++)
            {
                temp += vat[index].ToInt() * (index + 1);
            }

            total = temp % 11;

            if (total != 10)
            {
                return total == vat[8].ToInt();
            }

            temp = 0;
            for (var index = 0; index < 8; index++)
            {
                temp += vat[index].ToInt() * (index + 3);
            }

            total = temp % 11;

            if (total == 10)
            {
                total = 0;
            }

            return total == vat[8].ToInt();
        }

        private static bool BgPhysicalPerson(string vat)
        {
            if (!RegexPhysicalPerson.IsMatch(vat))
            {
                return false;
            }

            var month = int.Parse(vat.Slice(2, 2), CultureInfo.InvariantCulture);

            if ((month <= 0 || month >= 13) && (month <= 20 || month >= 33) && (month <= 40 || month >= 53))
            {
                return false;
            }

            var total = vat.Sum(MultipliersPhysicalPerson);

            total %= 11;

            if (total == 10)
            {
                total = 0;
            }

            return total == vat[9].ToInt();
        }

        private static bool BgForeignerPhysicalPerson(string vat)
        {
            var total = vat.Sum(MultipliersForeignPhysicalPerson);

            return total % 10 == vat[9].ToInt();
        }

        private static bool BgMiscellaneousVatNumber(string vat)
        {
            var total = vat.Sum(MultipliersMiscellaneous);

            total = 11 - total % 11;

            switch (total)
            {
                case 10:
                    return false;
                case 11:
                    total = 0;
                    break;
            }

            return total == vat[9].ToInt();
        }
    }
}