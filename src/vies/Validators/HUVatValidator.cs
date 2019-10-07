using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    internal sealed class HUVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{8}$";
            
        private static readonly int[] Multipliers = {9, 7, 3, 1, 9, 7, 3};

        public HUVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.HU);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var checkDigit = 10 - sum % 10;
            
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[7].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid HU vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}