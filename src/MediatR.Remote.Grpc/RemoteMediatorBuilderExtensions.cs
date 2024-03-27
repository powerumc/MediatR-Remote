using Grpc.Net.ClientFactory;
using MediatR.Remote.Grpc.RemoteStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Remote.Grpc;

/// <summary>
///     Extensions Grpc remote mediator for <see cref="RemoteMediatorBuilder" />.
/// </summary>
public static class RemoteMediatorBuilderExtensions
{
    /// <summary>
    ///     Add Grpc remote mediator.
    /// </summary>
    /// <param name="builder">A Builder object</param>
    /// <param name="name">Role name</param>
    /// <param name="configureClient">Configure <see cref="HttpClient" /></param>
    /// <param name="serviceLifetime">Service lifetime</param>
    public static RemoteMediatorBuilder AddGrpcStrategy(this RemoteMediatorBuilder builder, string name,
        Action<GrpcClientFactoryOptions> configureClient, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var protocolRoleName = new ProtocolRoleName("grpc", name);
        builder.Add<RemoteGrpcStrategy, RemoteGrpcStrategy, RemoteGrpcStrategy>(protocolRoleName, serviceLifetime);
        builder.Services.AddGrpcClient<MediatorGrpcService.MediatorGrpcServiceClient>(protocolRoleName.ToString(),
            configureClient);

        return builder;
    }
}
