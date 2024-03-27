using MediatR.Remote.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace MediatR.Remote.Grpc;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGrpcListener(this RemoteMediatorApplicationBuilder builder)
    {
        builder.WebApplication.MapGrpcService<MediatorGrpcServiceImpl>();

        return builder.WebApplication;
    }
}
