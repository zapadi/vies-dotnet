using System;
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

using System.Globalization;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LUVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{8}$";

        public LUVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.LU);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var isValid = int.Parse(vat.Substring(0, 6)) % 89 == int.Parse(vat.Substring(6, 2));
            return !isValid 
                ? VatValidationResult.Failed("Invalid LU vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}