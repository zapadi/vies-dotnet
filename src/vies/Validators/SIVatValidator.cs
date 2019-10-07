using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SIVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^[1-9]\d{7}$";
        private static readonly int[] Multipliers = {8, 7, 6, 5, 4, 3, 2};

        public SIVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.SI);
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
                ? VatValidationResult.Failed("Invalid SI vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}