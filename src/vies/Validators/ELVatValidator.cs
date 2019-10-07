using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ELVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{9}$";
        private static readonly int[] Multipliers = {256, 128, 64, 32, 16, 8, 4, 2};

        public ELVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.EL);    
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (vat.Length == 8)
            {
                vat = "0" + vat;
            }

            var sum = vat.Sum(Multipliers);

            var checkDigit = sum % 11;
            
            if (checkDigit > 9)
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