using System.Text.RegularExpressions;

namespace Padi.Vies
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class VatValidatorAbstract : IVatValidator
    {
        protected Regex Regex;
        public static string CountryCode { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vat"></param>
        /// <returns></returns>
        /// <exception cref="ViesValidationException"></exception>
        public VatValidationResult Validate(string vat)
        {
            if (Regex == null)
            {
                throw new ViesValidationException("The regex to validate format is null.");
            }
            
            return !Regex.IsMatch(vat) 
                ? VatValidationResult.Failed($"Invalid {CountryCode} vat: format") 
                : OnValidate(vat);
        }
        protected abstract VatValidationResult OnValidate(string vat);
    }
}