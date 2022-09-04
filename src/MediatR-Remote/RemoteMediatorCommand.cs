using System.Text.Json.Serialization;
using MediatR.Remote.Json;

namespace MediatR.Remote;

public class RemoteMediatorCommand : IRequest<RemoteMediatorResult>, INotification
{
    [JsonConstructor]
    public RemoteMediatorCommand(object? @object, IEnumerable<string>? spans = null)
    {
        Object = @object;
        Spans = spans ?? Enumerable.Empty<string>();
    }


    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }

    public IEnumerable<string>? Spans { get; }
}

public class RemoteMediatorResult
{
    [JsonConstructor]
    public RemoteMediatorResult(object? @object)
    {
        Object = @object;
    }

    [JsonConverter(typeof(ObjectPropertyJsonConverter))]
    public object? Object { get; }
}