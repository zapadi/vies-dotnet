using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EEVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^10\d{7}$"; //[0-9]{9}
        private static readonly int[] Multipliers = {3, 7, 1, 3, 7, 1, 3, 7};

        public EEVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.EE);
        }
        
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);
            
            var checkDigit = 10 - sum % 10;
            
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == vat[8].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid EE vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}