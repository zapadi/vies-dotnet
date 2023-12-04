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
    public sealed class ESVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^([A-Z]\d{8})$|^([A-HN-SW]\d{7}[A-J])$|^([0-9YZ]\d{7}[A-Z])|([KLMX]\d{7}[A-Z])$";

        public ESVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
            CountryCode = nameof(EuCountryCode.ES);
        }

        protected override VatValidationResult OnValidate(string vat)
        {
            return VatValidationResult.Success();
        }
    }
}