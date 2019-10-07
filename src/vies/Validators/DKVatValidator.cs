using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DKVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^(\d{2} ?){3}\d{2}$";
        private static readonly int[] Multipliers = {2, 7, 6, 5, 4, 3, 2, 1};

        public DKVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.DK);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            var isValid = sum % 11 == 0;
            return !isValid 
                ? VatValidationResult.Failed("Invalid DK vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}