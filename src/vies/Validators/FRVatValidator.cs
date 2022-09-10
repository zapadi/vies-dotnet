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
    public sealed class FRVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"[0-9A-Z]{2}[0-9]{9}";

        public FRVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.FR);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var validationKey = vat.Substring(0, 2);
            var val = int.Parse(vat.Substring(2));
            
            if (!int.TryParse(validationKey, out var temp))
            {
                return VatValidationResult.Success();
            }
          
            var checkDigit = ( 12 + 3 * ( val % 97 ) ) % 97;

            var isValid = checkDigit == temp;
            return !isValid 
                ? VatValidationResult.Failed("Invalid FR vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}