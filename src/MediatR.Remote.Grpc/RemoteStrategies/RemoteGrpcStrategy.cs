using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Grpc.Net.ClientFactory;
using MediatR.Remote.RemoteStrategies;

namespace MediatR.Remote.Grpc.RemoteStrategies;

/// <summary>
///     A remote strategy that uses HTTP to send the request to the target role.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RemoteGrpcStrategy : RemoteStrategyBase
{
    private readonly GrpcClientFactory _grpcClientFactory;

    public RemoteGrpcStrategy(GrpcClientFactory grpcClientFactory)
    {
        _grpcClientFactory = grpcClientFactory;
    }

    protected override async Task<RemoteMediatorResult?> SendInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var payload = new GrpcCommandRequest { Type = "", Object = JsonSerializer.Serialize(nextCommand) };
        var client = _grpcClientFactory.CreateClient<MediatorGrpc.MediatorGrpcClient>(targetRoleName);
        var response = await client.GrpcCommandServiceAsync(payload, cancellationToken: cancellationToken);
        var result = JsonSerializer.Deserialize<RemoteMediatorResult>(response.Object);

        return result;
    }

    protected override async Task NotificationInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var payload = new GrpcNotificationRequest { Type = "", Object = JsonSerializer.Serialize(nextCommand) };
        var client = _grpcClientFactory.CreateClient<MediatorGrpc.MediatorGrpcClient>(targetRoleName);
        await client.GrpcNotificationServiceAsync(payload, cancellationToken: cancellationToken);
    }
}
