using System.Reflection;
using MediatR.Remote.Extensions.DependencyInjection;
using MediatR.Remote.Grpc;
using Messages;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));

// Configure HTTP
services.AddRemoteMediatR("internal-api1", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("http").GetChildren())
    {
        remoteBuilder.AddHttpStrategy(section.Key, client => client.BaseAddress = new Uri(section.Value!));
    }
});

// Configure gRPC
services.AddGrpc();
services.AddRemoteMediatR<IGrpcMediator, GrpcMediator>("internal-api1", "grpc", remoteBuilder =>
{
    foreach (var section in builder.Configuration.GetRequiredSection("grpc").GetChildren())
    {
        remoteBuilder.AddGrpcStrategy(section.Key, client => client.Address = new Uri(section.Value!));
    }
});

var app = builder.Build();
app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseRemoteMediatR(routeBuilder => routeBuilder.MapHttpListener().AllowAnonymous());
app.UseRemoteMediatR(applicationBuilder => applicationBuilder.UseGrpcListener());

app.Run();
