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
    /// <summary>
    /// 
    /// </summary>
    public sealed class PLVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^\d{10}$";
        private static readonly int[] Multipliers = {6, 5, 7, 2, 3, 4, 5, 6, 7};
        
        public PLVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.PL);    
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var checkDigit = sum % 11;

            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[9].ToInt();
            
            return !isValid 
                ? VatValidationResult.Failed("Invalid PL vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}