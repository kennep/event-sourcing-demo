using System.Text.Json.Serialization;
using UserService.Events;

namespace UserService.Domain;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; private set; }

    [JsonPropertyName("terminatedAt")]
    public DateTimeOffset? TerminatedAt { get; private set; }

    private List<Identifier> _identifiers = new();

    [JsonPropertyName("identifiers")]
    public IReadOnlyList<Identifier> Identifiers => _identifiers;

    [JsonPropertyName("name")]
    public Name Name { get; private set; } = new("", "");

    [JsonPropertyName("state")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserState State { get; private set; } = UserState.Active;

    public User(string id)
    {
        Id = id;
    }

    public void ApplyEvent(Event userEvent)
    {
        switch (userEvent.Type)
        {
            case EventType.Create:
                CreatedAt = userEvent.Timestamp;
                break;
            case EventType.SetIdentifier:
                if (!_identifiers.Contains(userEvent.Identifier ??
                                           throw new InvalidOperationException(
                                               "setIdentifier event must have identifier populated")))
                {
                    _identifiers.Add(userEvent.Identifier );
                }
                break;
            case EventType.ExpireIdentifier:
                _identifiers.Remove(userEvent.Identifier ??
                                    throw new InvalidOperationException(
                                        "expireIdentifier event must have identifier populated"));
                break;
            case EventType.SetName:
                Name = userEvent.Name ?? throw new InvalidOperationException("setName event must have name populated");
                break;
            case EventType.Terminate:
                State = UserState.Terminated;
                TerminatedAt = userEvent.Timestamp;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ApplyEvents(EventList eventList)
    {
        foreach(var userEvent in eventList.Events)
        {
            ApplyEvent(userEvent);
        }
    }
}

public enum UserState
{
    Active,
    Terminated
}