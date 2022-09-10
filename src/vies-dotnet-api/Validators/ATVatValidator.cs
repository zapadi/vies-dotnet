/*
   Copyright 2017-2022 Adrian Popescu.
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

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ATVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^U\d{8}$";
        private static readonly int[] Multipliers = {1, 2, 1, 2, 1, 2, 1};
        
        /// <summary>
        /// 
        /// </summary>
        public ATVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
        
            CountryCode = nameof(EuCountryCode.AT);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var index = 1;
            var sum = 0;
            foreach (var digit in Multipliers)
            {
                var temp = vat[index++].ToInt() * digit;
                sum += temp > 9 ? (int) Math.Floor(temp / 10D) + temp % 10 : temp;
            }

            var checkDigit = 10 - (sum + 4) % 10;
            
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[8].ToInt();

            return !isValid 
                ? VatValidationResult.Failed($"Invalid {CountryCode} vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}