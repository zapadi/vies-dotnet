using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LUVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{8}$";

        public LUVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.LU);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var isValid = int.Parse(vat.Substring(0, 6)) % 89 == int.Parse(vat.Substring(6, 2));
            return !isValid 
                ? VatValidationResult.Failed("Invalid LU vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}