namespace UserService.Infrastructure;

public class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(string message, Exception cause) : base(message, cause)
    {

    }

}