using Confluent.Kafka;

namespace MediatR.Remote.Kafka;

public class KafkaMediatorOptions
{
    public IProducer<string, string> Producer { get; set; }

    public Func<IServiceProvider, RemoteMediatorCommand, string> MessageKeyGenerator =>
        (sp, command) => Guid.NewGuid().ToString();

    public IConsumer<string, string> Consumer { get; set; }

    public IAdminClient AdminClient { get; set; }

    public TimeSpan ConsumeTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
