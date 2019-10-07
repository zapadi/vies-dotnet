using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PTVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{9}$";
        private static readonly int[] Multipliers = {9, 8, 7, 6, 5, 4, 3, 2};

        public PTVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.PT);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var checkDigit = 11 - sum % 11;
            
            if (checkDigit > 9)
            {
                checkDigit = 0;
            }
            var isValid = checkDigit == vat[8].ToInt();
            return !isValid 
                ? VatValidationResult.Failed("Invalid PT vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}