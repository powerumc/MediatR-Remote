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
builder.Services.AddRemoteMediatR("public-api", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("internal-api1", client => client.BaseAddress = new Uri("http://localhost:5010"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.UseMediatRemote(routeBuilder => routeBuilder.MapHttpListener().AllowAnonymous());

app.Run();
