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
services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.MediaTypeOptions.AddText("application/json");
});
services.Configure<KestrelServerOptions>(options =>
{
    options.ListenLocalhost(5010, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5011, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
services.AddRemoteMediatR("internal-api1", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("internal-api2", client => client.BaseAddress = new Uri("http://localhost:5020"));
});

services.AddGrpc();
services.AddRemoteMediatR<IGrpcMediator, GrpcMediator>("internal-api1", "grpc", remoteBuilder =>
{
    remoteBuilder.AddGrpcStrategy("internal-api2", client => client.Address = new Uri("http://localhost:5021"));
});

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
