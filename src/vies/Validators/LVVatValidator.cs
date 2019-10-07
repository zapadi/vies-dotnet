using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LVVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^\d{11}$";
        private static readonly int[] Multipliers = {9, 1, 4, 8, 3, 10, 2, 5, 7, 6};

        public LVVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.LV);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            // Only check the legal bodies
            if (Regex.IsMatch(vat, "/^[0-3]/"))
            {
                var result = Regex.IsMatch(vat, "^[0-3][0-9][0-1][0-9]");
                return !result 
                    ? VatValidationResult.Failed("Invalid LV vat: checkValue") 
                    : VatValidationResult.Success();
            }
            var sum = vat.Sum(Multipliers);

            var checkDigit = sum % 11;

            if (checkDigit == 4 && vat[0] == '9')
            {
                checkDigit -= 45;
            }

            if (checkDigit == 4)
            {
                checkDigit = 4 - checkDigit;
            }
            else
            {
                if (checkDigit > 4)
                {
                    checkDigit = 14 - checkDigit;
                }
                else
                {
                    if (checkDigit < 4)
                    {
                        checkDigit = 3 - checkDigit;
                    }
                }
            }

            var isValid = checkDigit == vat[10].ToInt();
            
            return !isValid 
                ? VatValidationResult.Failed("Invalid LV vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}