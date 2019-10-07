using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BEVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^0?\d{9}$";

        public BEVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.BE);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (vat.Length == 10 && vat[0] != '0')
            {
                return VatValidationResult.Failed("First character of 10 digit numbers should be 0.");
            }
            
            if (vat.Length == 9)
            {
                vat = vat.PadLeft(10, '0');
            }

            // Modulus 97 check on last nine digits
            var isValid = 97 - int.Parse(vat.Substring(0, 8)) % 97 == int.Parse(vat.Substring(8, 2));

            return !isValid
                ? VatValidationResult.Failed("Invalid BE vat: checkValue.")
                : VatValidationResult.Success();
        }
    }
}