// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;

namespace MediatR.Remote.Grpc;

public class MediatorGrpcService(MediatorRemoteEndpoint endpoint) : MediatorGrpc.MediatorGrpcBase
{
    public override async Task<GrpcCommandResult> GrpcCommandService(GrpcCommandRequest request,
        ServerCallContext context)
    {
        var command = JsonSerializer.Deserialize<RemoteMediatorCommand>(request.Object)!;
        var result = await endpoint.InvokeAsync(command, context.CancellationToken);

        return new GrpcCommandResult { Type = request.Type, Object = JsonSerializer.Serialize(result) };
    }

    public override async Task<Empty> GrpcNotificationService(GrpcNotificationRequest request,
        ServerCallContext context)
    {
        var command = JsonSerializer.Deserialize<RemoteMediatorCommand>(request.Object)!;
        await endpoint.InvokeAsync(command, context.CancellationToken);
        return new Empty();
    }

    public override async Task GrpcStreamService(GrpcStreamCommandRequest request,
        IServerStreamWriter<GrpcStreamCommandResult> responseStream, ServerCallContext context)
    {
        var command = JsonSerializer.Deserialize<RemoteMediatorStreamCommand>(request.Object)!;
        var results = endpoint.InvokeStreamAsync(command, context.CancellationToken);

        await foreach (var result in results)
        {
            var grpcResult =
                new GrpcStreamCommandResult { Type = request.Type, Object = JsonSerializer.Serialize(result) };
            await responseStream.WriteAsync(grpcResult, context.CancellationToken);
        }
    }
}
