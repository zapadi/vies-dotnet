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

using System.Text.RegularExpressions;
using Padi.Vies.Errors;

namespace Padi.Vies;

/// <summary>
/// 
/// </summary>
public abstract class VatValidatorAbstract : IVatValidator
{
    protected Regex Regex { get; set; }
    public static string CountryCode { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vat"></param>
    /// <returns></returns>
    /// <exception cref="ViesValidationException"></exception>
    public VatValidationResult Validate(string vat)
    {
        if (this.Regex == null)
        {
            throw new ViesValidationException("The regex to validate format is null.");
        }
            
            return !Regex.IsMatch(vat) 
                ? VatValidationResult.Failed($"Invalid {CountryCode} vat: format") 
                : OnValidate(vat);
        }
    protected abstract VatValidationResult OnValidate(string vat);
}