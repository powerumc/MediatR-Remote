using MediatR.Remote.RemoteStrategies;

namespace MediatR.Remote.Extensions.DependencyInjection;

/// <summary>
/// Extensions HTTP remote mediator for <see cref="RemoteMediatorBuilder"/>.
/// </summary>
public static class RemoteMediatorBuilderExtensions
{
    /// <summary>
    /// Add HTTP remote mediator.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <param name="name">Role name</param>
    /// <param name="configureClient">Configure <see cref="HttpClient"/></param>
    /// <param name="serviceLifetime">Service lifetime</param>
    public static RemoteMediatorBuilder AddHttpStrategy(this RemoteMediatorBuilder builder, string name,
        Action<HttpClient> configureClient, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        builder.Add<RemoteHttpStrategy, RemoteHttpStrategy, RemoteHttpStrategy>(name, serviceLifetime);

        builder.Services.AddHttpClient(name, configureClient);

        return builder;
    }
}
