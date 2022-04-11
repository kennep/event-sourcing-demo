namespace UserService.Infrastructure;

public interface IRepository
{
    Task<Document<T>> CreateItem<T>(Document<T> document, CancellationToken cancellationToken);
    Task<Document<T>> GetItem<T>(string id, CancellationToken cancellationToken);
    Task<Document<T>> UpdateItem<T>(Document<T> document, CancellationToken cancellationToken);
}