using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class DEVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^[1-9]\d{8}$"; //[0-9]{9}

        public DEVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.DE);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var product = 10;
            for (var index = 0; index < 8; index++)
            {
                var sum = (vat[index].ToInt() + product) % 10;
                if (sum == 0)
                {
                    sum = 10;
                }

                product = 2 * sum % 11;
            }

            var val = 11 - product;
            var checkDigit = val == 10
                ? 0
                : val;

            var isValid = checkDigit == vat[8].ToInt();

            return !isValid
                ? VatValidationResult.Failed("Invalid DE vat: checkValue")
                : VatValidationResult.Success();
        }
    }
}