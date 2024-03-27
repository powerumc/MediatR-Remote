using System.Text.Json.Serialization;
using MediatR.Remote.Json;

namespace MediatR.Remote;

/// <summary>
///     A protocol for sending a stream request to a remote mediator.
/// </summary>
public class RemoteMediatorStreamCommand : IStreamRequest<RemoteMediatorStreamResult>
{
    [JsonConstructor]
    public RemoteMediatorStreamCommand(object? @object, string protocolName, IEnumerable<string>? spans = null)
    {
        Object = @object;
        ProtocolName = protocolName;
        Spans = spans ?? Enumerable.Empty<string>();
    }

    /// <summary>
    ///     The object to send to the remote mediator.
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }

    public string ProtocolName { get; }

    /// <summary>
    ///     A list of roles to send to the remote mediator.
    /// </summary>
    public IEnumerable<string>? Spans { get; }
}

/// <summary>
///     A protocol for receiving a stream response from a remote mediator.
/// </summary>
public class RemoteMediatorStreamResult
{
    [JsonConstructor]
    public RemoteMediatorStreamResult(object? @object)
    {
        Object = @object;
    }

    /// <summary>
    ///     The object received from the remote mediator.
    /// </summary>
    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }
}
