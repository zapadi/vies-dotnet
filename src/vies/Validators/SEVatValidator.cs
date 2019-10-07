using System;
using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SEVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{10}01$";
        private static readonly int[] Multipliers = {2, 1, 2, 1, 2, 1, 2, 1, 2};

        public SEVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.SE);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var index = 0;
            var sum = 0;
            foreach (var m in Multipliers)
            {
                var temp = vat[index++].ToInt() * m;
                sum += temp > 9 ? (int) Math.Floor(temp / 10D) + temp % 10 : temp;
            }

            var checkDigit = 10 - sum % 10;
            
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[9].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid SE vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}