// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR.Remote.Grpc.Endpoints;

namespace MediatR.Remote.Grpc;

public class MediatorGrpcService : MediatorGrpc.MediatorGrpcBase
{
    private readonly MediatorRemoteEndpoint _endpoint;

    public MediatorGrpcService(MediatorRemoteEndpoint endpoint)
    {
        _endpoint = endpoint;
    }

    public override async Task<GrpcCommandResult> GrpcCommandService(GrpcCommandRequest request,
        ServerCallContext context)
    {
        var result = await _endpoint.InvokeAsync(context.GetHttpContext(), request.Object);

        return new GrpcCommandResult { Type = request.Type, Object = JsonSerializer.Serialize(result) };
    }

    public override async Task<Empty> GrpcNotificationService(GrpcNotificationRequest request,
        ServerCallContext context)
    {
        await _endpoint.InvokeAsync(context.GetHttpContext(), request.Object);
        return new Empty();
    }

    public override Task GrpcStreamService(GrpcCommandRequest request,
        IServerStreamWriter<GrpcCommandResult> responseStream, ServerCallContext context)
    {
        return base.GrpcStreamService(request, responseStream, context);
    }
}
