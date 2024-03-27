using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Remote;

public class RemoteMediatorBuilder(IServiceCollection services)
{
    internal readonly IDictionary<ProtocolRoleName, StrategyTypes> Strategies =
        new Dictionary<ProtocolRoleName, StrategyTypes>();

    public IServiceCollection Services => services;

    /// <summary>
    ///     Json serializer options
    /// </summary>
    internal JsonSerializerOptions JsonSerializerOptions { get; } = new();

    /// <summary>
    ///     Default HTTP endpoint
    /// </summary>
    internal static string MediatorRemoteEndpoint => "mediator-remote";

    /// <summary>
    ///     Default streaming HTTP endpoint
    /// </summary>
    internal static string MediatorStreamRemoteEndpoint => "mediator-stream-remote";

    /// <summary>
    ///     Remote serializer
    /// </summary>
    public IRemoteSerializer? Serializer { get; set; }

    public RemoteMediatorBuilder OverrideJsonSerializerOptions(Action<JsonSerializerOptions> options)
    {
        options(JsonSerializerOptions);

        return this;
    }

    /// <summary>
    ///     Add a strategy with the specified name.
    /// </summary>
    /// <param name="protocolRoleName">Protocol and role name</param>
    /// <param name="serviceLifetime">Service lifetime</param>
    /// <typeparam name="TRequestStrategy">Request object strategy</typeparam>
    /// <typeparam name="TNotificationStrategy">Notification object strategy</typeparam>
    /// <typeparam name="TStreamStrategy">Stream object strategy</typeparam>
    /// <returns></returns>
    public RemoteMediatorBuilder Add<TRequestStrategy, TNotificationStrategy, TStreamStrategy>(
        ProtocolRoleName protocolRoleName, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TRequestStrategy : IRemoteStrategy
    {
        var strategyItem = new StrategyTypes(typeof(TRequestStrategy), typeof(TNotificationStrategy),
            typeof(TStreamStrategy));
        Strategies.Add(protocolRoleName, strategyItem);
        AddServices(strategyItem, serviceLifetime);

        return this;
    }

    private void AddServices(StrategyTypes strategyTypes, ServiceLifetime serviceLifetime)
    {
        Services.TryAdd(ServiceDescriptor.Describe(
            strategyTypes.RequestStrategyType, strategyTypes.RequestStrategyType, serviceLifetime));
        Services.TryAdd(ServiceDescriptor.Describe(
            strategyTypes.NotificationStrategyType, strategyTypes.NotificationStrategyType, serviceLifetime));
        Services.TryAdd(ServiceDescriptor.Describe(
            strategyTypes.StreamStrategyType, strategyTypes.StreamStrategyType, serviceLifetime));
    }
}

public sealed class ProtocolRoleName(string protocolName, string name)
{
    public string ProtocolName => protocolName;
    public string Name => name;

    public override string ToString()
    {
        return Generate(protocolName, name);
    }

    public static string Generate(string protocolName, string name)
    {
        return $"{name}_{protocolName}";
    }

    private bool Equals(ProtocolRoleName other)
    {
        return ProtocolName == other.ProtocolName && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ProtocolRoleName)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProtocolName, Name);
    }
}
