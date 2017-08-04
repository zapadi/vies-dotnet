namespace Zapadi.Vies
{
    public class ValidationException : System.Exception
    {
        public ValidationException() { }
        public ValidationException( string message ) : base( message ) { }
        public ValidationException( string message, System.Exception inner ) : base( message, inner ) { }
    }       
}