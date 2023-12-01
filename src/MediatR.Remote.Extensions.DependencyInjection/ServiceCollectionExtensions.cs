using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Remote.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up mediator remote services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add mediator remote services.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="myRoleName">My role name</param>
    /// <param name="configure">Configure <see cref="RemoteMediatorBuilder"/></param>
    /// <returns></returns>
    public static IServiceCollection AddRemoteMediatR(this IServiceCollection services, string myRoleName,
        Action<RemoteMediatorBuilder>? configure = null)
    {
        return AddRemoteMediatR(services, new[] { myRoleName }, configure);
    }

    /// <summary>
    /// Add mediator remote services.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="myRoleNames">My role name</param>
    /// <param name="configure">Configure <see cref="RemoteMediatorBuilder"/></param>
    public static IServiceCollection AddRemoteMediatR(this IServiceCollection services, IEnumerable<string> myRoleNames,
        Action<RemoteMediatorBuilder>? configure = null)
    {
        var builder = new RemoteMediatorBuilder(services);
        configure?.Invoke(builder);

        services.TryAddTransient<IRemoteMediator, RemoteMediator>();
        services.TryAddSingleton<IMediatorInvoker, MediatorInvoker>();
        services.TryAddTransient(
            typeof(IRequestHandler<RemoteMediatorCommand, RemoteMediatorResult?>), 
            typeof(RemoteMediatorCommandHandler));
        services.TryAddTransient(
            typeof(IStreamRequestHandler<RemoteMediatorStreamCommand, RemoteMediatorStreamResult>),
            typeof(RemoteMediatorStreamCommandHandler));
        services.TryAddTransient(
            typeof(INotificationHandler<RemoteMediatorCommand>),
            typeof(RemoteMediatorCommandHandler));
        services.TryAddTransient<MediatorRemoteEndpoint>();
        services.Configure<RemoteMediatorOptions>(options =>
        {
            options.MyRoleNames = myRoleNames.ToList().AsReadOnly();
            options.MediatorRemoteEndpoint = builder.MediatorRemoteEndpoint;
            options.RemoteStrategies = builder.Strategies;
            options.JsonSerializerOptions = builder.JsonSerializerOptions;
        });

        return services;
    }
}
