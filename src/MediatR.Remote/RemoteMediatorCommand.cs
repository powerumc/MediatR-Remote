using System.Text.Json.Serialization;
using MediatR.Remote.Json;

namespace MediatR.Remote;

/// <summary>
///     A protocol for sending a request to a remote mediator.
/// </summary>
[method: JsonConstructor]
public class RemoteMediatorCommand(object? @object, IEnumerable<string>? spans = null)
    : IRequest<RemoteMediatorResult>, INotification
{
    /// <summary>
    ///     <see cref="IMediator" /> command object
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; } = @object;

    /// <summary>
    ///     A list of roles to be included in the response.
    /// </summary>
    public IEnumerable<string>? Spans { get; } = spans ?? Enumerable.Empty<string>();
}

/// <summary>
///     A protocol for sending a response from a remote mediator.
/// </summary>
[method: JsonConstructor]
public class RemoteMediatorResult(object? @object)
{
    /// <summary>
    ///     <see cref="IMediator" /> command result object
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; } = @object;
}
