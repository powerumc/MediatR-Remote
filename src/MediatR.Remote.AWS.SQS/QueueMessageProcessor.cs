using Amazon.SQS;
using Amazon.SQS.Model;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.AWS.SQS;

public interface IQueueMessageProcessor
{
    Task CreateQueueIfNotExistsAsync(string roleName, CancellationToken cancellationToken);

    Task AcknowledgeMessageAsync(AwsSqsOptions options, Message message, CancellationToken cancellationToken);

    Task OnMessageAsync(RemoteMediatorCommand command, CancellationToken cancellationToken);

    Task OnMessageExceptionAsync(AwsSqsOptions options, Message message, Exception exception,
        CancellationToken cancellationToken);
}

public class QueueMessageProcessor(
    IServiceProvider serviceProvider,
    MediatorRemoteEndpoint endpoint,
    IOptionsMonitor<AwsSqsOptions> sqsOptions,
    ILogger<QueueListener> logger) : IQueueMessageProcessor
{
    public async Task CreateQueueIfNotExistsAsync(string roleName, CancellationToken cancellationToken)
    {
        var protocolRoleName = ProtocolRoleName.Generate("aws-sqs", roleName);
        var options = sqsOptions.Get(protocolRoleName);

        try
        {
            var queueName = roleName;
            var attributes = options.AttributesOnCreateQueue(serviceProvider);
            attributes.TryGetValue(QueueAttributeName.FifoQueue, out var fifoQueueValue);
            if (bool.TryParse(fifoQueueValue, out var isFifoQueue) &&
                isFifoQueue &&
                !queueName.EndsWith(".fifo"))
            {
                queueName += ".fifo";
            }

            var createQueueRequest = new CreateQueueRequest { QueueName = queueName, Attributes = attributes };
            var response = await options.Client.CreateQueueAsync(createQueueRequest, cancellationToken);

            logger.LogInformation("Queue {RoleName} created successfully. QueueUrl: {QueueUrl}", roleName,
                response.QueueUrl);
        }
        catch (QueueNameExistsException)
        {
            logger.LogInformation($"Queue {roleName} already exists");
        }
    }

    public async Task AcknowledgeMessageAsync(AwsSqsOptions options, Message message,
        CancellationToken cancellationToken)
    {
        var deleteMessageRequest = new DeleteMessageRequest
        {
            QueueUrl = options.QueueUrl, ReceiptHandle = message.ReceiptHandle
        };
        await options.Client.DeleteMessageAsync(deleteMessageRequest, cancellationToken);

        logger.LogInformation("Message {MessageId} acknowledged", message.MessageId);
    }

    public async Task OnMessageAsync(RemoteMediatorCommand command, CancellationToken cancellationToken)
    {
        await endpoint.InvokeAsync(command, cancellationToken);
    }

    public async Task OnMessageExceptionAsync(AwsSqsOptions options, Message message, Exception exception,
        CancellationToken cancellationToken)
    {
        var request = new ChangeMessageVisibilityRequest
        {
            QueueUrl = options.QueueUrl,
            ReceiptHandle = message.ReceiptHandle,
            VisibilityTimeout = (int)TimeSpan.FromMinutes(1).TotalSeconds
        };
        await options.Client.ChangeMessageVisibilityAsync(request, cancellationToken);

        logger.LogError(exception, "Message {MessageId} processing failed", message.MessageId);
    }
}
