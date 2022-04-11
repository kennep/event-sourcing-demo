using System.Text.Json.Serialization;

namespace UserService.Events;

public record Event
{
    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EventType Type { get; }

    [JsonPropertyName("identifier")]
    public Identifier? Identifier { get; }

    [JsonPropertyName("name")]
    public Name? Name { get; }

    public Event(string id, DateTimeOffset timestamp, EventType type)
    {
        Id = id;
        Timestamp = timestamp;
        Type = type;
    }

    [JsonConstructor]
    public Event(
        string id,
        DateTimeOffset timestamp,
        EventType type,
        Identifier? identifier,
        Name? name)
    {
        Id = id;
        Timestamp = timestamp;
        Type = type;
        Identifier = identifier;
        Name = name;
    }


}

public record Name(
    [property: JsonPropertyName("givenName")] string GivenName,
    [property: JsonPropertyName("familyName")] string FamilyName
);

public record Identifier(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("authority")] string Authority,
    [property: JsonPropertyName("value")] string Value
);

public enum EventType
{
    Create,
    SetIdentifier,
    ExpireIdentifier,
    SetName,
    Terminate
}

public record EventList
{
    private List<Event> _events;

    [JsonPropertyName("events")]
    public IEnumerable<Event> Events => _events;

    public EventList()
    {
        _events = new List<Event>();
    }

    [JsonConstructor]
    public EventList(
        IEnumerable<Event> events
    )
    {
        _events = events.ToList();
    }

    public bool Add(Event newEvent)
    {
        foreach (var existingEvent in _events)
        {
            if (existingEvent.Id == newEvent.Id)
            {
                return false;
            }
        }

        _events.Add(newEvent);
        return true;
    }
}