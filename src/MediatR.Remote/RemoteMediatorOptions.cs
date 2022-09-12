using System.Text.Json;

namespace MediatR.Remote;

public class RemoteMediatorOptions
{
    public IReadOnlyCollection<string> MyRoleNames { get; set; } = null!;
    public string MediatorRemoteEndpoint { get; set; } = null!;

    public IDictionary<string, StrategyTypes> RemoteStrategies { get; set; } = null!;
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = null!;
}