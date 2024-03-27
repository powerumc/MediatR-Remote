using System.Text.Json.Serialization;
using MediatR.Remote.Json;

namespace MediatR.Remote;

/// <summary>
///     A protocol for sending a stream request to a remote mediator.
/// </summary>
[method: JsonConstructor]
public class RemoteMediatorStreamCommand(object? @object, string protocolName, IEnumerable<string>? spans = null)
    : IStreamRequest<RemoteMediatorStreamResult>
{
    /// <summary>
    ///     The object to send to the remote mediator.
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; } = @object;

    public string ProtocolName { get; } = protocolName;

    /// <summary>
    ///     A list of roles to send to the remote mediator.
    /// </summary>
    public IEnumerable<string>? Spans { get; } = spans ?? Enumerable.Empty<string>();
}

/// <summary>
///     A protocol for receiving a stream response from a remote mediator.
/// </summary>
[method: JsonConstructor]
public class RemoteMediatorStreamResult(object? @object)
{
    /// <summary>
    ///     The object received from the remote mediator.
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; } = @object;
}
