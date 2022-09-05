using System.Text.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Remote.Extensions.DependencyInjection;

public class RemoteMediatorBuilder
{
    internal readonly JsonSerializerOptions JsonSerializerOptions = new();
    internal readonly IServiceCollection Services;
    internal readonly IDictionary<string, StrategyTypes> Strategies = new Dictionary<string, StrategyTypes>();


    public RemoteMediatorBuilder(IServiceCollection services)
    {
        Services = services;
    }

    internal string MediatorRemoteEndpoint { get; private set; } = "/mediator-remote";

    public RemoteMediatorBuilder OverrideEndpoint(string endpoint)
    {
        MediatorRemoteEndpoint = endpoint;

        return this;
    }

    public RemoteMediatorBuilder OverrideJsonSerializerOptions(Action<JsonSerializerOptions> options)
    {
        options(JsonSerializerOptions);

        return this;
    }

    public RemoteMediatorBuilder Add<TRequestStrategy, TNotificationStrategy, TStreamStrategy>(string name,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        where TRequestStrategy : IRemoteStrategy
    {
        var strategyItem = new StrategyTypes(typeof(TRequestStrategy), typeof(TNotificationStrategy),
            typeof(TStreamStrategy));
        Strategies.Add(name, strategyItem);
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