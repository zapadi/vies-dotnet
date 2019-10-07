using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    internal sealed class IEVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^(\d{7}[A-W])|([7-9][A-Z\*\+)]\d{5}[A-W])|(\d{7}[A-W][AH])$";
        private static readonly Regex RegexType2 = new Regex(@"/^\d[A-Z\*\+]/", RegexOptions.Compiled);
        private static readonly Regex RegexType3 = new Regex(@"/^\d{7}[A-Z][AH]$/", RegexOptions.Compiled);
        
        private static readonly int[] Multipliers = {8, 7, 6, 5, 4, 3, 2};


        public IEVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.IE);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (RegexType2.IsMatch(vat))
            {
                vat = "0" + vat.Substring(2, 7) 
                          + vat.Substring(0, 1) 
                          + vat.Substring(7, 8);
            }

            var sum = vat.Sum(Multipliers);

            // If the number is type 3 then we need to include the trailing A or H in the calculation
            if (RegexType3.IsMatch(vat))
            {
                // Add in a multiplier for the character A (1*9=9) or H (8*9=72)
                if (vat[8] == 'H')
                {
                    sum += 72;
                }
                else
                {
                    sum += 9;
                }
            }

            var checkDigit = sum % 23;
            
            var isValid = vat[7] == (checkDigit == 0 ? 'W' : (char) (checkDigit + 64));
            
            return !isValid 
                ? VatValidationResult.Failed("Invalid IE vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}