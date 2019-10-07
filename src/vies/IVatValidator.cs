namespace Padi.Vies
{
    /// <summary>
    /// 
    /// </summary>
    public interface IVatValidator
    {
        VatValidationResult Validate(string vat);
    }
}