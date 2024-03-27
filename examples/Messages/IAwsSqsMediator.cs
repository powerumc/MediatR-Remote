using MediatR;
using MediatR.Remote;

namespace Messages;

public interface IAwsSqsMediator : IRemoteMediator;

public class AwsSqsMediator(IMediator mediator) : RemoteMediator(mediator), IAwsSqsMediator
{
    public override string ProtocolName => "aws-sqs";
}
