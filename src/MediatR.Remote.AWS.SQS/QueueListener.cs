using Amazon.SQS.Model;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.AWS.SQS;

public class QueueListener(
    MediatorRemoteEndpoint endpoint,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<AwsSqsOptions> sqsOptions,
    ILogger<QueueListener> logger)
    : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Starting {nameof(QueueListener)}");

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Stopping {nameof(QueueListener)}");

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
                    logger.LogError(e, "Queue does not exist");
                    throw;
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
                await endpoint.InvokeAsync(command!, cancellationToken);
                await DeleteMessageAsync(options, message, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while invoking command: {MessageId}", message.MessageAttributes);
            }
        }
    }

    private static async Task DeleteMessageAsync(AwsSqsOptions options, Message message,
        CancellationToken cancellationToken)
    {
        var request = new DeleteMessageRequest(options.QueueUrl, message.ReceiptHandle);
        await options.Client.DeleteMessageAsync(request, cancellationToken);
    }
}
