using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FIVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{8}$";
        private static readonly int[] Multipliers = {7, 9, 10, 5, 8, 4, 2};

        public FIVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.FI);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var checkDigit = 11 - sum % 11;
            
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[7].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid FI vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}