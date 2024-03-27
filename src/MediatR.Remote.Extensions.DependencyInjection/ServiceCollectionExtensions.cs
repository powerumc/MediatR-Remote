using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Remote.Extensions.DependencyInjection;

/// <summary>
///     Extension methods for setting up mediator remote services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add mediator remote services.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="myRoleName">My role name</param>
    /// <param name="configure">Configure <see cref="RemoteMediatorBuilder" /></param>
    /// <returns></returns>
    public static IServiceCollection AddRemoteMediatR(this IServiceCollection services, string myRoleName,
        Action<RemoteMediatorBuilder>? configure = null)
    {
        return AddRemoteMediatR<IRemoteMediator, RemoteMediator>(services, new[] { myRoleName }, "http", configure);
    }

    /// <summary>
    ///     Add mediator remote services.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="myRoleName">My role name</param>
    /// <param name="protocolName">Protocol name</param>
    /// <param name="configure">Configure <see cref="RemoteMediatorBuilder" /></param>
    /// <returns></returns>
    public static IServiceCollection AddRemoteMediatR<TMediatorInterface, TMediatorImpl>(
        this IServiceCollection services,
        string myRoleName,
        string protocolName,
        Action<RemoteMediatorBuilder>? configure = null)
        where TMediatorInterface : IRemoteMediator
        where TMediatorImpl : TMediatorInterface
    {
        return AddRemoteMediatR<TMediatorInterface, TMediatorImpl>(services, new[] { myRoleName }, protocolName,
            configure);
    }

    /// <summary>
    ///     Add mediator remote services.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="myRoleNames">My role name</param>
    /// <param name="protocolName">Protocol name</param>
    /// <param name="configure">Configure <see cref="RemoteMediatorBuilder" /></param>
    /// <typeparam name="TMediatorInterface">Your custom interface that inherited <see cref="RemoteMediator" /></typeparam>
    /// <typeparam name="TMediatorImpl">Your custom implementation that inherited</typeparam>
    public static IServiceCollection AddRemoteMediatR<TMediatorInterface, TMediatorImpl>(
        this IServiceCollection services,
        IEnumerable<string> myRoleNames,
        string protocolName,
        Action<RemoteMediatorBuilder>? configure = null)
        where TMediatorInterface : IRemoteMediator
        where TMediatorImpl : TMediatorInterface
    {
        var builder = new RemoteMediatorBuilder(services);
        configure?.Invoke(builder);

        services.TryAddTransient(
            typeof(TMediatorInterface),
            typeof(TMediatorImpl));
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
        services.Configure<RemoteMediatorOptions>(protocolName, options =>
        {
            options.ProtocolName = protocolName;
            options.MyRoleNames = myRoleNames.ToList().AsReadOnly();
            options.MediatorRemoteEndpoint = RemoteMediatorBuilder.MediatorRemoteEndpoint;
            options.MediatorStreamRemoteEndpoint = RemoteMediatorBuilder.MediatorStreamRemoteEndpoint;
            options.RemoteStrategies = builder.Strategies;
            options.JsonSerializerOptions = builder.JsonSerializerOptions;
        });

        return services;
    }
}
