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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CYVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^([0-59]\d{7}[A-Z])$";//[0-9]{8}L

        public CYVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));    
            CountryCode = nameof(EuCountryCode.CY);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (int.Parse(vat.Slice(0, 2), CultureInfo.InvariantCulture) == 12)
            {
                return VatValidationResult.Failed("CY vat first 2 characters cannot be 12");
            }

            var result = 0;
            for (var index = 0; index < 8; index++)
            {
                var temp = vat[index].ToInt();
                
                if (index % 2 == 0)
                {
                    switch (temp)
                    {
                        case 0:
                            temp = 1;
                            break;
                        case 1:
                            temp = 0;
                            break;
                        case 2:
                            temp = 5;
                            break;
                        case 3:
                            temp = 7;
                            break;
                        case 4:
                            temp = 9;
                            break;
                        default:
                            temp = temp * 2 + 3;
                            break;
                    }
                }
                result += temp;
            }

            var checkDigit = result % 26;
            var isValid = Convert.ToInt32(vat[8]) == checkDigit + 65;

            return !isValid 
                ? VatValidationResult.Failed("Invalid CY vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}