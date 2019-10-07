using System.Text.RegularExpressions;

namespace Padi.Vies.Validators
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ESVatValidator : VatValidatorAbstract
    {
        private const string RegexPattern =@"^([A-Z]\d{8})|([A-HN-SW]\d{7}[A-J])|([0-9YZ]\d{7}[A-Z])|([KLMX]\d{7}[A-Z])$";

        public ESVatValidator()
        {
            Regex = new Regex(RegexPattern, RegexOptions.Compiled);
            CountryCode = nameof(EuCountryCode.ES);
        }

        protected override VatValidationResult OnValidate(string vat)
        {
            return VatValidationResult.Success();
        }
    }
}