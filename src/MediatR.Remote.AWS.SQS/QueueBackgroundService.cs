using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.AWS.SQS;

public class QueueBackgroundService(
    IQueueMessageProcessor messageProcessor,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<AwsSqsOptions> sqsOptions,
    ILogger<QueueBackgroundService> logger)
    : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Starting {nameof(QueueBackgroundService)}");

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Stopping {nameof(QueueBackgroundService)}");

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mediatorOptions = remoteMediatorOptions.Get("aws-sqs");

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var roleName in mediatorOptions.MyRoleNames)
            {
                var protocolRoleName = ProtocolRoleName.Generate("aws-sqs", roleName);
                var options = sqsOptions.Get(protocolRoleName);
                var queueUrl = options.QueueUrl;

                try
                {
                    var receiveMessageRequest = new ReceiveMessageRequest
                    {
                        QueueUrl = queueUrl,
                        WaitTimeSeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
                        MaxNumberOfMessages = 10
                    };
                    var receiveMessageResponse = await options.Client.ReceiveMessageAsync(receiveMessageRequest,
                        stoppingToken);

                    if (!receiveMessageResponse.Messages.Any())
                    {
                        continue;
                    }

                    await InvokeNotificationsAsync(mediatorOptions, options, receiveMessageResponse.Messages,
                        stoppingToken);
                }
                catch (QueueDoesNotExistException e)
                {
                    await messageProcessor.CreateQueueIfNotExistsAsync(roleName, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while receiving messages");
                }
            }
        }
    }

    private async Task InvokeNotificationsAsync(RemoteMediatorOptions mediatorOptions, AwsSqsOptions options,
        List<Message> messages,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Received {Count} messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var command = await mediatorOptions.Serializer.DeserializeFromStringAsync<RemoteMediatorCommand>(
                    message.Body, cancellationToken);
                await messageProcessor.OnMessageAsync(command!, cancellationToken);
                await messageProcessor.AcknowledgeMessageAsync(options, message, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while invoking command: {MessageId}", message.MessageAttributes);
                await messageProcessor.OnMessageExceptionAsync(options, message, e, cancellationToken);
            }
        }
    }
}
