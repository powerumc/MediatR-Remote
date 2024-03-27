using MediatR.Remote.RemoteStrategies;

namespace MediatR.Remote.Extensions.DependencyInjection;

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