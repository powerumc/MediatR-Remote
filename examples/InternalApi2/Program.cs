using System.Net;
using System.Reflection;
using Amazon.Runtime;
using Amazon.SQS;
using MediatR.Remote.AWS.SQS;
using MediatR.Remote.Extensions.DependencyInjection;
using MediatR.Remote.Grpc;
using Messages;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.Configure<KestrelServerOptions>(options =>
{
    options.Listen(IPAddress.Any, 5020, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
    options.Listen(IPAddress.Any, 5021, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

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

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseRemoteMediatR(routeBuilder => routeBuilder.MapHttpListener().AllowAnonymous());
app.UseRemoteMediatR(applicationBuilder => applicationBuilder.UseGrpcListener());

app.Run();
