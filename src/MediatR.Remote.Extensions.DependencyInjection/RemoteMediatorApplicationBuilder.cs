namespace MediatR.Remote.Extensions.DependencyInjection;

public class RemoteMediatorApplicationBuilder
{
    public RemoteMediatorApplicationBuilder(IApplicationBuilder applicationBuilder)
    {
        ApplicationBuilder = applicationBuilder;
    }

    public IApplicationBuilder ApplicationBuilder { get; }
}

public class RemoteMediatorEndpointRouteBuilder
{
    public RemoteMediatorEndpointRouteBuilder(IEndpointRouteBuilder endpointRouteBuilder)
    {
        EndpointRouteBuilder = endpointRouteBuilder;
    }

    public IEndpointRouteBuilder EndpointRouteBuilder { get; }
}
