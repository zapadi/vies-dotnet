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
    public sealed class BEVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^0?\d{9}$";

        public BEVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(5));    
            CountryCode = nameof(EuCountryCode.BE);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (vat.Length == 10 && vat[0] != '0')
            {
                return VatValidationResult.Failed("First character of 10 digit numbers should be 0.");
            }
            
            if (vat.Length == 9)
            {
                vat = vat.PadLeft(10, '0');
            }

            // Modulus 97 check on last nine digits
            var isValid = 97 - int.Parse(vat.Substring(0, 8)) % 97 == int.Parse(vat.Substring(8, 2));

            return !isValid
                ? VatValidationResult.Failed("Invalid BE vat: checkValue.")
                : VatValidationResult.Success();
        }
    }
}