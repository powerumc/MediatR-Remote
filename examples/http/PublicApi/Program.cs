using System.Reflection;
using MediatR;
using MediatR.Remote.Extensions.DependencyInjection;
using Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly);
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

app.UseRouting();
app.UseMediatRemote(mediatorApplicationBuilder => mediatorApplicationBuilder.UseHttpListener());

app.Run();
