using System.Reflection;
using MediatR.Remote.Extensions.DependencyInjection;
using MediatR.Remote.Grpc;
using Messages;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.All);
services.Configure<KestrelServerOptions>(options =>
{
    options.ListenLocalhost(5020, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5021, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
services.AddRemoteMediatR("internal-api2");

services.AddGrpc();
services.AddRemoteMediatR<IGrpcMediator, GrpcMediator>("internal-api2", "grpc");

var app = builder.Build();
app.UseHttpLogging();
app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseRemoteMediatR(mediatorApplicationBuilder =>
{
    mediatorApplicationBuilder.UseHttpListener();
    mediatorApplicationBuilder.UseGrpcListener();
});

app.Run();
