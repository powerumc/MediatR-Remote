using MediatR;
using MediatR.Remote;

namespace Messages;

public interface IGrpcMediator : IRemoteMediator;

public class GrpcMediator(IMediator mediator) : RemoteMediator(mediator), IGrpcMediator
{
    public override string ProtocolName => "grpc";
}
