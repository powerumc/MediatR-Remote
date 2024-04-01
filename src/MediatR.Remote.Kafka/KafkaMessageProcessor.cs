using Confluent.Kafka.Admin;
using MediatR.Remote.Extensions.DependencyInjection.Endpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Kafka;

public class KafkaMessageProcessor(
    MediatorRemoteEndpoint endpoint,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<KafkaMediatorOptions> kafkaOptions,
    ILogger<KafkaMessageProcessor> logger) : IQueueMessageProcessor<KafkaMediatorOptions, string>
{
    public async Task CreateQueueIfNotExistsAsync(string roleName, CancellationToken cancellationToken)
    {
        try
        {
            var protocolRoleName = ProtocolRoleName.Generate("kafka", roleName);
            var options = kafkaOptions.Get(protocolRoleName);
            await options.AdminClient.CreateTopicsAsync(new TopicSpecification[]
            {
                new() { Name = roleName, ReplicationFactor = 1, NumPartitions = 5 }
            });
        }
        catch (CreateTopicsException e)
        {
            logger.LogError(e, "An error occured creating topics");
            throw;
        }
    }

    public Task AcknowledgeMessageAsync(KafkaMediatorOptions options, string message,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task OnMessageAsync(RemoteMediatorCommand command, CancellationToken cancellationToken)
    {
        await endpoint.InvokeAsync(command, cancellationToken);
    }

    public Task OnMessageExceptionAsync(KafkaMediatorOptions options, string message, Exception exception,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
