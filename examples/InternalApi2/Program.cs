using System.Reflection;
using Amazon.Runtime;
using Amazon.SQS;
using Confluent.Kafka;
using MediatR.Remote.AWS.SQS;
using MediatR.Remote.Extensions.DependencyInjection;
using MediatR.Remote.Grpc;
using MediatR.Remote.Kafka;
using MediatR.Remote.Redis;
using Messages;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));

// Configure HTTP
services.AddRemoteMediatR("internal-api2", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("http").GetChildren())
    {
        remoteBuilder.AddHttpStrategy(section.Key, client => client.BaseAddress = new Uri(section.Value!));
    }
});

services.AddGrpc();

// Configure gRPC
services.AddRemoteMediatR<IGrpcMediator, GrpcMediator>("internal-api2", "grpc", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("grpc").GetChildren())
    {
        remoteBuilder.AddGrpcStrategy(section.Key, client => client.Address = new Uri(section.Value!));
    }
});

// Configure AWS SQS
services.AddRemoteMediatR<IAwsSqsMediator, AwsSqsMediator>("internal-api2", "aws-sqs", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("aws-sqs").GetChildren())
    {
        remoteBuilder.AddSqsStrategy(section.Key,
            options =>
            {
                options.Client = new AmazonSQSClient(new BasicAWSCredentials("", ""),
                    new AmazonSQSConfig { ServiceURL = section.Value });
                options.MessageGroupIdGenerator = (_, _) => "hello";
                options.QueueUrl = section.Value!;
            });
    }
});

// Configure Redis
services.AddRemoteMediatR<IRedisMediator, RedisMediator>("internal-api2", "redis", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("redis").GetChildren())
    {
        remoteBuilder.AddRedisStrategy(section.Key, options =>
        {
            options.ConnectionMultiplexer = ConnectionMultiplexer.Connect(section.Value);
            options.Subscriber = (_, connectionMultiplexer) => connectionMultiplexer.GetSubscriber();
            options.SubscribeChannels = (_, roleName) =>
                new[] { RedisChannel.Literal($"{roleName}:Group1"), RedisChannel.Literal($"{roleName}:Group2") };
            options.MessageChannelGenerator = (_, _, targetRoleName) => $"{targetRoleName}:Group1";
        });
    }
});

// Configure Kafka
services.AddRemoteMediatR<IKafkaMediator, KafkaMediator>("internal-api2", "kafka", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("kafka").GetChildren())
    {
        remoteBuilder.AddKafkaStrategy(section.Key, options =>
        {
            options.Producer =
                new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = section.Value }).Build();
            options.Consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
            {
                BootstrapServers = section.Value, GroupId = section.Key, AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();
            options.AdminClient =
                new AdminClientBuilder(new AdminClientConfig { BootstrapServers = section.Value }).Build();
        });
    }
});

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseRemoteMediatR(routeBuilder => routeBuilder.MapHttpListener().AllowAnonymous());
app.UseRemoteMediatR(applicationBuilder => applicationBuilder.UseGrpcListener());
app.UseRemoteMediatR(applicationBuilder => applicationBuilder.UseRedisListener());

app.Run();
