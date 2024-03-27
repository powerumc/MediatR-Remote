using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using MediatR.Remote.RemoteStrategies;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Grpc.RemoteStrategies;

/// <summary>
///     A remote strategy that uses HTTP to send the request to the target role.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RemoteGrpcStrategy(
    GrpcClientFactory grpcClientFactory,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions)
    : RemoteStrategyBase
{
    protected override async Task<RemoteMediatorResult?> SendInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var client = CreateGrpcClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get("grpc");
        var json = await options.Serializer.SerializeAsStringAsync(nextCommand, cancellationToken);
        var payload = new GrpcCommandRequest { Object = json };
        var response = await client.GrpcCommandAsync(payload, cancellationToken: cancellationToken);
        var result = await options.Serializer.DeserializeFromStringAsync<RemoteMediatorResult>(response.Object,
            cancellationToken);

        return result;
    }

    protected override async Task NotificationInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var client = CreateGrpcClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get("grpc");
        var json = await options.Serializer.SerializeAsStringAsync(nextCommand, cancellationToken);
        var payload = new GrpcNotificationRequest { Object = json };
        await client.GrpcNotificationAsync(payload, cancellationToken: cancellationToken);
    }

    protected override async IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var client = CreateGrpcClient(targetRoleName, nextCommand);
        var options = remoteMediatorOptions.Get("grpc");
        var json = await options.Serializer.SerializeAsStringAsync(nextCommand, cancellationToken);
        var payload = new GrpcStreamCommandRequest { Object = json };
        using var serverStream = client.GrpcStream(payload, cancellationToken: cancellationToken);
        var stream = serverStream.ResponseStream.ReadAllAsync(cancellationToken);
        await foreach (var result in stream)
        {
            var item = await options.Serializer.DeserializeFromStringAsync<RemoteMediatorStreamResult>(result.Object,
                cancellationToken);
            yield return item;
        }
    }

    private MediatorGrpcService.MediatorGrpcServiceClient CreateGrpcClient(string targetRoleName,
        RemoteMediatorCommand nextCommand)
    {
        var grpcClientName = ProtocolRoleName.Generate(nextCommand.ProtocolName, targetRoleName);
        var client = grpcClientFactory.CreateClient<MediatorGrpcService.MediatorGrpcServiceClient>(grpcClientName);
        return client;
    }
}
