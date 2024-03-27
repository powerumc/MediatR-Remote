using MediatR.Remote.Grpc.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediatR.Remote.Grpc;

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
    /// <param name="configure">Configure <see cref="RemoteMediatorGrpcBuilder" /></param>
    /// <returns></returns>
    public static IServiceCollection AddRemoteGrpcMediatR(this IServiceCollection services, string myRoleName,
        Action<RemoteMediatorGrpcBuilder>? configure = null)
    {
        return AddRemoteGrpcMediatR<IRemoteMediator>(services, new[] { myRoleName }, configure);
    }

    /// <summary>
    ///     Add mediator remote services.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="myRoleNames">My role name</param>
    /// <param name="configure">Configure <see cref="RemoteMediatorGrpcBuilder" /></param>
    /// <typeparam name="TMediatorInterface">Your custom interface that inherited <see cref="RemoteMediator" /></typeparam>
    public static IServiceCollection AddRemoteGrpcMediatR<TMediatorInterface>(this IServiceCollection services,
        IEnumerable<string> myRoleNames,
        Action<RemoteMediatorGrpcBuilder>? configure = null)
        where TMediatorInterface : IRemoteMediator
    {
        var builder = new RemoteMediatorGrpcBuilder(services);
        configure?.Invoke(builder);

        services.AddGrpc();

        services.TryAddTransient(
            typeof(TMediatorInterface),
            typeof(RemoteMediator));
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
