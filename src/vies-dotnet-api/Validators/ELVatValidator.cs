/*
   Copyright 2017-2023 Adrian Popescu.
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
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators;

    /// <summary>
    /// 
    /// </summary>
    public sealed class ELVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{9}$";
        private static readonly int[] Multipliers = {256, 128, 64, 32, 16, 8, 4, 2};

        public ELVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled, TimeSpan.FromSeconds(5));    
            CountryCode = nameof(EuCountryCode.EL);    
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (vat.Length == 8)
            {
                vat = $"0{vat}";
            }

            var sum = vat.Sum(Multipliers);

            var checkDigit = sum % 11;
            
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[8].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid EE vat: checkValue") 
                : VatValidationResult.Success();
    }
}