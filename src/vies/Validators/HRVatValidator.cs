using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    internal sealed class HRVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{11}$";

        public HRVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.HR);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var product = 10;

            for (var index = 0; index < 10; index++)
            {
                int sum = (vat[index].ToInt() + product) % 10;
               
                if (sum == 0)
                {
                    sum = 10;
                }
                
                product = 2 * sum % 11;
            }

            var checkDigit = (product + vat[10].ToInt()) % 10;
            
            var isValid = checkDigit == 1;
            return !isValid 
                ? VatValidationResult.Failed("Invalid HR vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}