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
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);
        
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