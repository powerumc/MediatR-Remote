using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Kafka;

public class QueueBackgroundService(
    KafkaMessageProcessor messageProcessor,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<KafkaMediatorOptions> kafkaOptions,
    ILogger<QueueBackgroundService> logger) : BackgroundService
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Starting Kafka {nameof(QueueBackgroundService)}");

        var mediatorOptions = remoteMediatorOptions.Get("kafka");

        if (cancellationToken.IsCancellationRequested)
        {
            return base.StartAsync(cancellationToken);
        }

        foreach (var roleName in mediatorOptions.MyRoleNames)
        {
            var protocolRoleName = ProtocolRoleName.Generate("kafka", roleName);
            var options = kafkaOptions.Get(protocolRoleName);
            options.Consumer.Subscribe(roleName);

            logger.LogInformation($"Kafka {nameof(QueueBackgroundService)} subscribed to {roleName}");
        }

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Kafka {nameof(QueueBackgroundService)}");

        var mediatorOptions = remoteMediatorOptions.Get("kafka");

        foreach (var roleName in mediatorOptions.MyRoleNames)
        {
            var protocolRoleName = ProtocolRoleName.Generate("kafka", roleName);
            var options = kafkaOptions.Get(protocolRoleName);
            options.Consumer.Close();
        }

        return Task.CompletedTask;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken stoppingToken)
    {
        var mediatorOptions = remoteMediatorOptions.Get("kafka");

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var roleName in mediatorOptions.MyRoleNames)
            {
                var protocolRoleName = ProtocolRoleName.Generate("kafka", roleName);
                var options = kafkaOptions.Get(protocolRoleName);
                var result = await GetConsumes(options, roleName, stoppingToken);
                if (result is null)
                {
                    continue;
                }

                logger.LogInformation("Kafka {Name} received message: {Key}",
                    nameof(QueueBackgroundService), result.Message.Key);

                string? json = null;
                try
                {
                    json = result.Message.Value;
                    var command = await mediatorOptions.Serializer.DeserializeFromStringAsync<RemoteMediatorCommand>(
                        json, stoppingToken);
                    if (command is null)
                    {
                        continue;
                    }

                    await messageProcessor.OnMessageAsync(command, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while receiving messages");
                    await messageProcessor.OnMessageExceptionAsync(options, json, e, stoppingToken);
                }
            }
        }
    }

    private async Task<ConsumeResult<string, string>?> GetConsumes(KafkaMediatorOptions options,
        string roleName, CancellationToken stoppingToken)
    {
        try
        {
            return options.Consumer.Consume(options.ConsumeTimeout);
        }
        catch (ConsumeException e)
        {
            logger.LogInformation("Create kafka topic if not exists: {roleName}", roleName);
            await messageProcessor.CreateQueueIfNotExistsAsync(roleName, stoppingToken);
        }

        return null;
    }
}
