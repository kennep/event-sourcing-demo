using System.Text.Json.Serialization;

namespace UserService.Infrastructure;

public record Document<T>(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("etag")] string? ETag,
    [property: JsonPropertyName("data")] T Data);