using Amazon.SQS.Model;
using MediatR.Remote.RemoteStrategies;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.AWS.SQS.RemoteStrategies;

public class RemoteAwsSqsStrategy(
    IServiceProvider serviceProvider,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<AwsSqsOptions> sqsOptions) : RemoteStrategyBase
{
    protected override Task<RemoteMediatorResult?> SendInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    protected override async Task NotificationInternalAsync(string targetRoleName, RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var mediatorOptions = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var json = await mediatorOptions.Serializer.SerializeAsStringAsync(nextCommand, cancellationToken);
        var protocolRoleName = ProtocolRoleName.Generate(nextCommand.ProtocolName, targetRoleName);
        var options = sqsOptions.Get(protocolRoleName);
        var request = new SendMessageRequest(options.QueueUrl, json)
        {
            MessageGroupId = options.MessageGroupIdGenerator(serviceProvider, nextCommand),
            MessageDeduplicationId = options.MessageDeduplicationIdGenerator(serviceProvider, nextCommand)
        };
        await options.Client.SendMessageAsync(request, cancellationToken);
    }

    protected override IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
