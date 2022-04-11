using System.Text.Json;

namespace UserService.Infrastructure;

public class FileRepository : IRepository
{
    private string basePath;

    public FileRepository()
    {
        basePath = "db";
    }

    public async Task<Document<T>> CreateItem<T>(Document<T> document, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(basePath);
        var path = GetItemPath(document.Id);
        var storedDoc = document with { ETag = IdUtils.GenerateRandomId() };
        var serializedDoc = JsonSerializer.SerializeToUtf8Bytes(storedDoc);

        await File.WriteAllBytesAsync(path, serializedDoc, cancellationToken);
        return storedDoc;
    }

    public async Task<Document<T>> GetItem<T>(string id, CancellationToken cancellationToken)
    {
        var path = GetItemPath(id);
        try
        {
            var content = await File.ReadAllBytesAsync(path, cancellationToken);
            return JsonSerializer.Deserialize<Document<T>>(content) ?? throw
                new InvalidOperationException("Document is null");
        }
        catch (DirectoryNotFoundException e)
        {
            throw new DocumentNotFoundException("Document not found", e);
        }
        catch (FileNotFoundException e)
        {
            throw new DocumentNotFoundException("Document not found", e);
        }

    }

    public async Task<Document<T>> UpdateItem<T>(Document<T> document, CancellationToken cancellationToken)
    {
        // FIXME: there is a race condition in this code
        var docInRepo = await GetItem<T>(document.Id, cancellationToken);
        if (docInRepo.ETag != document.ETag)
        {
            throw new DocumentConcurrentModificationException("Concurrent modification");
        }

        Directory.CreateDirectory(basePath);
        var path = GetItemPath(document.Id);
        var storedDoc = document with { ETag = IdUtils.GenerateRandomId() };
        var serializedDoc = JsonSerializer.SerializeToUtf8Bytes(storedDoc);

        await File.WriteAllBytesAsync(path, serializedDoc, cancellationToken);
        return storedDoc;
    }

    private string GetItemPath(string id)
    {
        return Path.Join(basePath, id);
    }

}