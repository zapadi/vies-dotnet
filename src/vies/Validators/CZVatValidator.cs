using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CZVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^(\d{8,10})(\d{3})?$";
        private static readonly int[] Multipliers = {8, 7, 6, 5, 4, 3, 2};

        public CZVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.CZ);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            // Only do check digit validation for standard VAT numbers
            if (vat.Length != 8)
            {
                return VatValidationResult.Success();
            }

            var sum = vat.Sum(Multipliers);

            var checkDigit = 11 - sum % 11;

            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            if (checkDigit == 11)
            {
                checkDigit = 1;
            }

            var isValid = checkDigit == vat[7].ToInt();
            return !isValid
                ? VatValidationResult.Failed("Invalid CZ vat: checkValue")
                : VatValidationResult.Success();
        }
    }
}