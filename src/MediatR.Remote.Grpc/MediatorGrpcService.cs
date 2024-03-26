using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Grpc;

public class MediatorGrpcServiceImpl(
    MediatorRemoteEndpoint endpoint,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions) : MediatorGrpcService.MediatorGrpcServiceBase
{
    public override async Task<GrpcCommandResult> GrpcCommand(GrpcCommandRequest request,
        ServerCallContext context)
    {
        var options = remoteMediatorOptions.Get("grpc");
        var command = await options.Serializer.DeserializeFromStringAsync<RemoteMediatorCommand>(request.Object);
        var result = await endpoint.InvokeAsync(command!, context.CancellationToken);
        var obj = await options.Serializer.SerializeAsStringAsync(result);

        return new GrpcCommandResult { Object = obj };
    }

    public override async Task<Empty> GrpcNotification(GrpcNotificationRequest request,
        ServerCallContext context)
    {
        var options = remoteMediatorOptions.Get("grpc");
        var command = await options.Serializer.DeserializeFromStringAsync<RemoteMediatorCommand>(request.Object);
        await endpoint.InvokeAsync(command!, context.CancellationToken);
        return new Empty();
    }

    public override async Task GrpcStream(GrpcStreamCommandRequest request,
        IServerStreamWriter<GrpcStreamCommandResult> responseStream, ServerCallContext context)
    {
        var options = remoteMediatorOptions.Get("grpc");
        var command = await options.Serializer.DeserializeFromStringAsync<RemoteMediatorStreamCommand>(request.Object);
        var results = endpoint.InvokeStreamAsync(command!, context.CancellationToken);

        await foreach (var result in results)
        {
            var json = await options.Serializer.SerializeAsStringAsync(result);
            var grpcResult = new GrpcStreamCommandResult { Object = json };
            await responseStream.WriteAsync(grpcResult, context.CancellationToken);
        }
    }
}
