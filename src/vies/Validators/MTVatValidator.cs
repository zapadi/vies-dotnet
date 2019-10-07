using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    public sealed class MTVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^[1-9]\d{7}$";
        private static readonly int[] Multipliers = {3, 4, 6, 7, 8, 9};

        public MTVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.MT);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var checkDigit = 37 - sum % 37;

            var isValid = checkDigit == int.Parse(vat.Substring(6, 2));
            
            return !isValid 
                ? VatValidationResult.Failed("Invalid MT vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}