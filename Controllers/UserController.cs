using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using UserService.Domain;
using UserService.Events;
using UserService.Infrastructure;

namespace UserService.Controllers;

[ApiController]
[Route("/users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IRepository _repository;

    public UserController(ILogger<UserController> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUser(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var userEvents = await _repository.GetItem<EventList>(userId, cancellationToken);
            var user = new User(userId);
            user.ApplyEvents(userEvents.Data);

            return new ObjectResult(user);
        }
        catch (DocumentNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost()]
    public async Task<ActionResult> CreateUser(CancellationToken cancellationToken)
    {
        var userId = IdUtils.GenerateRandomId();
        var userEvents = new Document<EventList>(userId, null, new EventList(
            new[] { new Event(IdUtils.GenerateRandomId(), DateTimeOffset.Now, EventType.Create)}
            ));
        await _repository.CreateItem(userEvents, cancellationToken);
        var user = new User(userId);
        user.ApplyEvents(userEvents.Data);

        return new CreatedResult($"/users/{userId}", user);
    }

    public class IdentifierEventRequest
    {
        [JsonPropertyName("event")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public EventType EventType { get; set; }

        [JsonPropertyName("type")]
        [Required]
        public string Type { get; set; } = null!;

        [JsonPropertyName("authority")]
        [Required]
        public string Authority { get; set; } = null!;

        [JsonPropertyName("value")]
        [Required]
        public string Value { get; set; } = null!;
    }

    public record UserIdResponse(
        [property: JsonPropertyName("id")] string UserId);

    [HttpPost("{userId}/identifiers/{eventId}")]
    public async Task<ActionResult> SetIdentifier(string userId, string eventId, [FromBody] IdentifierEventRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.EventType != EventType.SetIdentifier && request.EventType != EventType.ExpireIdentifier)
            {
                return new BadRequestObjectResult("Event type must be SetIdentifier or ExpireIdentifier");
            }

            var newEvent = new Event(eventId, DateTimeOffset.Now, request.EventType,
                new Identifier(request.Type, request.Authority, request.Value), null);

            var userEvents = await _repository.GetItem<EventList>(userId, cancellationToken);
            var added = userEvents.Data.Add(newEvent);
            await _repository.UpdateItem(userEvents, cancellationToken);
            return added ? new CreatedResult($"/users/{userId}", new UserIdResponse(userId)) : new OkResult();
        }
        catch (DocumentNotFoundException)
        {
            return BadRequest();
        }
    }

    public class NameEventRequest
    {
        [JsonPropertyName("event")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public EventType EventType { get; set; }

        [JsonPropertyName("givenName")]
        [Required]
        public string GivenName { get; set; } = null!;

        [JsonPropertyName("familyName")]
        [Required]
        public string FamilyName { get; set; } = null!;
    }

    [HttpPost("{userId}/name/{eventId}")]
    public async Task<ActionResult> SetName(string userId, string eventId, [FromBody] NameEventRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.EventType != EventType.SetName)
            {
                return new BadRequestObjectResult("Event type must be SetName");
            }

            var newEvent = new Event(eventId, DateTimeOffset.Now, request.EventType,
                null, new Name(request.GivenName, request.FamilyName));

            var userEvents = await _repository.GetItem<EventList>(userId, cancellationToken);
            var added = userEvents.Data.Add(newEvent);
            await _repository.UpdateItem(userEvents, cancellationToken);
            return added ? new CreatedResult($"/users/{userId}", new UserIdResponse(userId)) : new OkResult();
        }
        catch (DocumentNotFoundException)
        {
            return BadRequest();
        }
    }

    [HttpPost("{userId}/terminate/{eventId}")]
    public async Task<ActionResult> TerminateUser(string userId, string eventId, CancellationToken cancellationToken)
    {
        try
        {
            var newEvent = new Event(eventId, DateTimeOffset.Now, EventType.Terminate);

            var userEvents = await _repository.GetItem<EventList>(userId, cancellationToken);
            var added = userEvents.Data.Add(newEvent);
            await _repository.UpdateItem(userEvents, cancellationToken);
            return added ? new CreatedResult($"/users/{userId}", new UserIdResponse(userId)) : new OkResult();
        }
        catch (DocumentNotFoundException)
        {
            return BadRequest();
        }
    }

}
