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
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    internal sealed class HUVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{8}$";
            
        private static readonly int[] Multipliers = {9, 7, 3, 1, 9, 7, 3};

        public HUVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.HU);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var checkDigit = 10 - sum % 10;
            
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[7].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid HU vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}