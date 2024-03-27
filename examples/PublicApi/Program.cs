using System.Net;
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
    options.Listen(IPAddress.Any, 5000, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(5001, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
services.AddRemoteMediatR("public-api", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("internal-api1", client => client.BaseAddress = new Uri("http://localhost:5010"));
});

services.AddGrpc();
services.AddRemoteMediatR<IGrpcMediator, GrpcMediator>("public-api", "grpc", remoteBuilder =>
{
    remoteBuilder.AddGrpcStrategy("internal-api1", client => client.Address = new Uri("http://localhost:5011"));
});

var app = builder.Build();
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.UseRemoteMediatR(routeBuilder => routeBuilder.MapHttpListener().AllowAnonymous());
app.UseRemoteMediatR(applicationBuilder => applicationBuilder.UseGrpcListener());

app.Run();
