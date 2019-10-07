using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FRVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"[0-9A-Z]{2}[0-9]{9}";

        public FRVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.FR);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var validationKey = vat.Substring(0, 2);
            var val = int.Parse(vat.Substring(2));
            
            if (!int.TryParse(validationKey, out var temp))
            {
                return VatValidationResult.Success();
            }
          
            var checkDigit = ( 12 + 3 * ( val % 97 ) ) % 97;

            var isValid = checkDigit == temp;
            return !isValid 
                ? VatValidationResult.Failed("Invalid FR vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}