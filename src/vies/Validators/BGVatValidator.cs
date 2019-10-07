using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BGVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern = @"^\d{9,10}$";
        private static readonly Regex RegexPhysicalPerson = new Regex(@"^\d\d[0-5]\d[0-3]\d\d{4}$", RegexOptions.Compiled);

        private static readonly int[] MultipliersPhysicalPerson = {2, 4, 8, 5, 10, 9, 7, 3, 6};
        private static readonly int[] MultipliersForeignPhysicalPerson = {21, 19, 17, 13, 11, 9, 7, 3, 1};
        private static readonly int[] MultipliersMiscellaneous = {4, 3, 2, 7, 6, 5, 4, 3, 2};

        public BGVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);    
            CountryCode = nameof(EuCountryCode.BG);
        }
        
        protected override VatValidationResult OnValidate(string vat)
        {
            bool isValid;
            if (vat.Length == 9)
            {
                isValid = Bg9DigitsVat(vat);
                return isValid ? VatValidationResult.Success() : VatValidationResult.Failed("Invalid 9 digits vat.");
            }

            if (BgPhysicalPerson(vat))
            {
                return VatValidationResult.Success();
            }

            isValid = BgForeignerPhysicalPerson(vat) || BgMiscellaneousVatNumber(vat);

            return !isValid
                ? VatValidationResult.Failed("")
                : VatValidationResult.Success();
        }

        private static bool Bg9DigitsVat(string vat)
        {
            var total = 0;
            var temp = 0;
            for (var index = 0; index < 8; index++)
            {
                temp += vat[index].ToInt() * (index + 1);
            }

            total = temp % 11;

            if (total != 10)
            {
                return total == vat[8].ToInt();
            }

            temp = 0;
            for (var index = 0; index < 8; index++)
            {
                temp += vat[index].ToInt() * (index + 3);
            }

            total = temp % 11;

            if (total == 10)
            {
                total = 0;
            }

            return total == vat[8].ToInt();
        }

        private static bool BgPhysicalPerson(string vat)
        {
            if (!RegexPhysicalPerson.IsMatch(vat))
            {
                return false;
            }

            var month = int.Parse(vat.Substring(2, 2));

            if ((month <= 0 || month >= 13) && (month <= 20 || month >= 33) && (month <= 40 || month >= 53))
            {
                return false;
            }

            var total = vat.Sum(MultipliersPhysicalPerson);

            total %= 11;

            if (total == 10)
            {
                total = 0;
            }

            return total == vat[9].ToInt();
        }

        private static bool BgForeignerPhysicalPerson(string vat)
        {
            var total = vat.Sum(MultipliersForeignPhysicalPerson);

            return total % 10 == vat[9].ToInt();
        }

        private static bool BgMiscellaneousVatNumber(string vat)
        {
            var total = vat.Sum(MultipliersMiscellaneous);

            total = 11 - total % 11;

            if (total == 10)
            {
                return false;
            }

            if (total == 11)
            {
                total = 0;
            }

            return total == vat[9].ToInt();
        }
    }
}