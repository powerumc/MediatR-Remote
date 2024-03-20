using System.Reflection;
using MediatR.Remote.Grpc;
using MediatR.Remote.Grpc.Endpoints;
using Messages;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5011, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
builder.Services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
builder.Services.AddRemoteGrpcMediatR("internal-api1", remoteBuilder =>
{
    remoteBuilder.AddGrpcStrategy("internal-api2", client => client.Address = new Uri("http://localhost:5021"));
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseRemoteGrpcMediatR();

app.Run();
