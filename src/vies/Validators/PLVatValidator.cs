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