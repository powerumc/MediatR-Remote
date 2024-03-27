using System.Reflection;
using MediatR;
using MediatR.Remote.Extensions.DependencyInjection;
using Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly);
builder.Services.AddRemoteMediatR("internal-api2");

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseMediatRemote(mediatorApplicationBuilder => mediatorApplicationBuilder.UseHttpListener());

app.Run();
