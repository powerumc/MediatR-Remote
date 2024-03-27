using System.Reflection;
using MediatR.Remote.Grpc;
using MediatR.Remote.Grpc.Endpoints;
using Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblies = new[] { Assembly.GetExecutingAssembly(), typeof(HelloRemoteRequest).Assembly };
builder.Services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(assemblies));
builder.Services.AddRemoteGrpcMediatR("public-api", remoteBuilder =>
{
    remoteBuilder.AddGrpcStrategy("internal-api1", client => client.Address = new Uri("http://localhost:5011"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.UseRemoteGrpcMediatR();

app.Run();
