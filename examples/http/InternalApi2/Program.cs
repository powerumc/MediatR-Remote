using System.Reflection;
using MediatR;
using MediatR.Remote.Extensions.DependencyInjection;
using Messages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly);
builder.Services.AddRemoteMediatR("internal-api2", remoteBuilder =>
{
    remoteBuilder.AddHttpStrategy("public-api", client => client.BaseAddress = new Uri("http://localhost:5000"));
    remoteBuilder.AddHttpStrategy("internal-api1", client => client.BaseAddress = new Uri("http://localhost:5010"));
    remoteBuilder.AddHttpStrategy("internal-api2", client => client.BaseAddress = new Uri("http://localhost:5020"));
});

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.UseRouting();
app.UseMediatRemote(builder => builder.UseHttpListener());

app.Run();