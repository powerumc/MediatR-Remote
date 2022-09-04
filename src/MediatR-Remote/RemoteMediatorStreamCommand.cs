using System.Text.Json.Serialization;
using MediatR.Remote.Json;

namespace MediatR.Remote;

public class RemoteMediatorStreamCommand : IStreamRequest<RemoteMediatorStreamResult>
{
    [JsonConstructor]
    public RemoteMediatorStreamCommand(object? @object, IEnumerable<string>? spans = null)
    {
        Object = @object;
        Spans = spans ?? Enumerable.Empty<string>();
    }


    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }

    public IEnumerable<string>? Spans { get; }
}

public class RemoteMediatorStreamResult
{
    [JsonConstructor]
    public RemoteMediatorStreamResult(object? @object)
    {
        Object = @object;
    }

    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }
}