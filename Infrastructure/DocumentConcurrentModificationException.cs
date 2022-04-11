namespace UserService.Infrastructure;

public class DocumentConcurrentModificationException : Exception
{
    public DocumentConcurrentModificationException(string message, Exception cause) : base(message, cause)
    {

    }

    public DocumentConcurrentModificationException(string message) : base(message)
    {

    }

}