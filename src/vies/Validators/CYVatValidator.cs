using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CYVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^([0-59]\d{7}[A-Z])$";//[0-9]{8}L

        public CYVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.CY);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            if (int.Parse(vat.Substring(0, 2)) == 12)
            {
                return VatValidationResult.Failed("CY vat first 2 characters cannot be 12");
            }

            var result = 0;
            for (var index = 0; index < 8; index++)
            {
                var temp = vat[index].ToInt();
                
                if (index % 2 == 0)
                {
                    switch (temp)
                    {
                        case 0:
                            temp = 1;
                            break;
                        case 1:
                            temp = 0;
                            break;
                        case 2:
                            temp = 5;
                            break;
                        case 3:
                            temp = 7;
                            break;
                        case 4:
                            temp = 9;
                            break;
                        default:
                            temp = temp * 2 + 3;
                            break;
                    }
                }
                result += temp;
            }

            var checkDigit = result % 26;
            var isValid = vat[8].ToInt() == checkDigit + 65;

            return !isValid 
                ? VatValidationResult.Failed("Invalid CY vat: checkValue") 
                : VatValidationResult.Success();
        }
    }
}