using System.Text.Json.Serialization;
using MediatR.Remote.Json;

namespace MediatR.Remote;

/// <summary>
/// A protocol for sending a request to a remote mediator.
/// </summary>
public class RemoteMediatorCommand : IRequest<RemoteMediatorResult>, INotification
{
    [JsonConstructor]
    public RemoteMediatorCommand(object? @object, IEnumerable<string>? spans = null)
    {
        Object = @object;
        Spans = spans ?? Enumerable.Empty<string>();
    }


    /// <summary>
    /// <see cref="IMediator"/> command object
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }

    /// <summary>
    /// A list of roles to be included in the response.
    /// </summary>
    public IEnumerable<string>? Spans { get; }
}

/// <summary>
/// A protocol for sending a response from a remote mediator.
/// </summary>
public class RemoteMediatorResult
{
    [JsonConstructor]
    public RemoteMediatorResult(object? @object)
    {
        Object = @object;
    }

    /// <summary>
    /// <see cref="IMediator"/> command result object
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }
}
