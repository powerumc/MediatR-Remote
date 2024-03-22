using System.Reflection;
using MediatR.Remote.Extensions.DependencyInjection;
using Messages;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.All);

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
builder.Services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
builder.Services.AddRemoteMediatR("internal-api1", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("internal-api2", client => client.BaseAddress = new Uri("http://localhost:5020"));
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseRemoteMediatR(mediatorApplicationBuilder => mediatorApplicationBuilder.UseHttpListener());

app.Run();
