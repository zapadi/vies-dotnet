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
    internal sealed class GBVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{9}|\d{12}|(GD|HA)\d{3}$";
            
        private static readonly int[] Multipliers = {8, 7, 6, 5, 4, 3, 2};

        public GBVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.GB);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
             var prefix = vat.Substring(0, 2);
            int no;
            if (prefix == "GD" && int.TryParse(vat.Substring(2, 3), out no))
            {
                //    return no < 500;
            }
            
            if (prefix == "HA" && int.TryParse(vat.Substring(2, 3), out no))
            {
                //  return no > 499;
            }
            
            if (prefix == "HA")
            {
                return VatValidationResult.Failed("");
            }


            if (vat[0].ToInt() == 0)
            {
                return VatValidationResult.Failed("0 VAT numbers disallowed");
            }

            // Check range is OK for modulus 97 calculation
            no = int.Parse(vat.Substring(0, 7));

            var total = vat.Sum(Multipliers);

            // Old numbers use a simple 97 modulus, but new numbers use an adaptation of that (less 55). Our 

            // Establish check digits by subtracting 97 from total until negative.
            var cd = total;
            while (cd > 0)
            {
                cd -= 97;
            }

            // Get the absolute value and compare it with the last two characters of the VAT number. If the 
            // same, then it is a valid traditional check digit. However, even then the number must fit within
            // certain specified ranges.
            cd = Math.Abs(cd);
            if (cd == int.Parse(vat.Substring(7, 2)) && no < 9990001 && (no < 100000 || no > 999999) &&
                (no < 9490001 || no > 9700000))
            {
                return VatValidationResult.Success();
            }

            // Now try the new method by subtracting 55 from the check digit if we can - else add 42
            if (cd >= 55)
            {
                cd -= 55;
            }
            else
            {
                cd += 42;
            }
            
            var isValid = cd == int.Parse(vat.Substring(7, 2)) && no > 1000000;
            return !isValid 
                ? VatValidationResult.Failed("Invalid GB vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}