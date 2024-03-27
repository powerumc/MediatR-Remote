using System.Text.Json;
using MediatR.Remote.RemoteStrategies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Remote.Extensions.DependencyInjection;

public class RemoteMediatorBuilder
{
    internal readonly JsonSerializerOptions JsonSerializerOptions = new();
    internal readonly IServiceCollection Services;
    internal readonly IDictionary<string, StrategyItem> Strategies = new Dictionary<string, StrategyItem>();


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
        var strategyItem = new StrategyItem(typeof(TRequestStrategy), typeof(TNotificationStrategy),
            typeof(TStreamStrategy));
        Strategies.Add(name, strategyItem);
        AddServices(strategyItem, serviceLifetime);

        return this;
    }

    private void AddServices(StrategyItem strategyItem, ServiceLifetime serviceLifetime)
    {
        Services.TryAdd(ServiceDescriptor.Describe(
            strategyItem.RequestStrategyType, strategyItem.RequestStrategyType, serviceLifetime));
        Services.TryAdd(ServiceDescriptor.Describe(
            strategyItem.NotificationStrategyType, strategyItem.NotificationStrategyType, serviceLifetime));
        Services.TryAdd(ServiceDescriptor.Describe(
            strategyItem.StreamStrategyType, strategyItem.StreamStrategyType, serviceLifetime));
    }
}

public static class RemoteMediatorBuilderExtensions
{
    public static RemoteMediatorBuilder AddHttpStrategy(this RemoteMediatorBuilder builder, string name,
        Action<HttpClient> configureClient, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        builder.Add<RemoteHttpStrategy, RemoteHttpStrategy, RemoteHttpStrategy>(name, serviceLifetime);

        builder.Services.AddHttpClient(name, configureClient);

        return builder;
    }
}