using Confluent.Kafka;
using MediatR.Remote.RemoteStrategies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediatR.Remote.Kafka.RemoteStrategies;

public class RemoteKafkaStrategy(
    IServiceProvider serviceProvider,
    IOptionsMonitor<RemoteMediatorOptions> remoteMediatorOptions,
    IOptionsMonitor<KafkaMediatorOptions> kafkaOptions,
    ILogger<RemoteKafkaStrategy> logger) : RemoteStrategyBase
{
    protected override Task<RemoteMediatorResult?> SendInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override async Task NotificationInternalAsync(string targetRoleName, RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        var mediatorOptions = remoteMediatorOptions.Get(nextCommand.ProtocolName);
        var json = await mediatorOptions.Serializer.SerializeAsStringAsync(nextCommand, cancellationToken);
        var protocolRoleName = ProtocolRoleName.Generate(nextCommand.ProtocolName, targetRoleName);
        var options = kafkaOptions.Get(protocolRoleName);
        var key = options.MessageKeyGenerator(serviceProvider, nextCommand);
        var message = new Message<string, string> { Key = key, Value = json };

        logger.LogInformation("Kafka sending message: {Key}", key);
        await options.Producer.ProduceAsync(targetRoleName, message, cancellationToken);
    }

    protected override IAsyncEnumerable<RemoteMediatorStreamResult?> StreamInternalAsync(string targetRoleName,
        RemoteMediatorCommand nextCommand,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
