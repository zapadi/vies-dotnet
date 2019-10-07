using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SKVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^[1-9]\d[2346-9]\d{7}$";

        public SKVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);
            CountryCode = nameof(EuCountryCode.SK);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var nr = ulong.Parse(vat);
            var isValid = nr % 11 == 0;
            return !isValid 
                ? VatValidationResult.Failed("Invalid SK vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}