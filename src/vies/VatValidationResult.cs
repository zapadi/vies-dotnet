namespace Padi.Vies
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class VatValidationResult
    {
        private VatValidationResult()
        {
            
        }

        public static VatValidationResult Success()
        {
            return new VatValidationResult(){IsValid = true};
        }

        public static VatValidationResult Failed(string errorMessage)
        {
            return new VatValidationResult(){Error = errorMessage};
        }

        public bool IsValid { get; private set; }

        public string Error { get; private set; }
    }
}