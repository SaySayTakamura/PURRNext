
// Creates a custom exception for graceful shutdown implementation on docker deploys
// Taken from:
// Link: https://learn.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions
public class SigtermRequestException : Exception
{
    public SigtermRequestException(){}
    public SigtermRequestException(string message) : base(message){}
    public SigtermRequestException(string message, Exception inner) : base(message, inner) {}
}