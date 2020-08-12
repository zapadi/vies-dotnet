using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class NLVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^\d{9}B\d{2}$";
        private static readonly int[] Multipliers = { 9, 8, 7, 6, 5, 4, 3, 2 };

        public NLVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);
            CountryCode = nameof(EuCountryCode.NL);
        }

        protected override VatValidationResult OnValidate(string vat)
        {
            var sum = vat.Sum(Multipliers);

            // Old VAT numbers (pre 2020) - Modulus 11 test
            var checkMod11 = sum % 11;
            if (checkMod11 > 9)
            {
                checkMod11 = 0;
            }
            var isValidMod11 = checkMod11 == vat[8].ToInt();
            if (isValidMod11)
            {
                return VatValidationResult.Success();
            }


            // New VAT numbers (post 2020) - Modulus 97 test
            const string stringValueNl = "2321";
            const string stringValueB = "11";
            vat = vat.Replace("B", stringValueB);

            var isValidMod97 = long.Parse(stringValueNl + vat) % 97 == 1;

            return !isValidMod97
                ? VatValidationResult.Failed("Invalid NL vat: checkValue")
                : VatValidationResult.Success();
        }
    }
}