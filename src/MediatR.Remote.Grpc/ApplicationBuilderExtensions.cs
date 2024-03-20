using Microsoft.AspNetCore.Builder;

namespace MediatR.Remote.Grpc;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRemoteGrpcMediatR(this WebApplication app)
    {
        app.MapGrpcService<MediatorGrpcService>();

        return app;
    }
}
