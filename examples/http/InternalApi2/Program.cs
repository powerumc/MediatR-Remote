using System.Reflection;
using MediatR;
using MediatR.Remote.Extensions.DependencyInjection;
using Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
builder.Services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
builder.Services.AddRemoteMediatR("internal-api2");

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseMediatRemote(mediatorApplicationBuilder => mediatorApplicationBuilder.UseHttpListener());

app.Run();
