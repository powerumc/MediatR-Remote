using System.Text.Json;

namespace MediatR.Remote;

/// <summary>
/// Options for the <see cref="RemoteMediator"/>
/// </summary>
public class RemoteMediatorOptions
{
    /// <summary>
    /// The name of the role that this mediator is in.
    /// </summary>
    public IReadOnlyCollection<string> MyRoleNames { get; set; } = null!;
    
    /// <summary>
    /// Remote mediator endpoint.
    /// </summary>
    public string MediatorRemoteEndpoint { get; set; } = null!;

    /// <summary>
    /// Remote mediator communication strategy.
    /// </summary>
    public IDictionary<string, StrategyTypes> RemoteStrategies { get; set; } = null!;
    
    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use when serializing and deserializing messages.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = null!;
}
