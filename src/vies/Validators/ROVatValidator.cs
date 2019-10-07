using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ROVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^[0-9]{2,10}$";
        private static readonly int[] Multipliers = { 7, 5, 3, 2, 1, 7, 5, 3, 2 };

        public ROVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.RO);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            var end = vat.Length - 1;
            
            var controlDigit = vat[end].ToInt();

            var slice = vat.Slice(0, end);

            vat = slice.PadLeft(9, '0');

            var sum = vat.Sum(Multipliers);

            var checkDigit = sum * 10 % 11;
                
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }

            var isValid = checkDigit == controlDigit;
            return !isValid 
                ? VatValidationResult.Failed("Invalid RO vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}