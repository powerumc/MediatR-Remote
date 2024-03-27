namespace MediatR.Remote.Extensions.DependencyInjection;

public class RemoteMediatorApplicationBuilder
{
    public RemoteMediatorApplicationBuilder(WebApplication webApplication)
    {
        WebApplication = webApplication;
    }

    public WebApplication WebApplication { get; }
}

public class RemoteMediatorEndpointRouteBuilder
{
    public RemoteMediatorEndpointRouteBuilder(IEndpointRouteBuilder endpointRouteBuilder)
    {
        EndpointRouteBuilder = endpointRouteBuilder;
    }

    public IEndpointRouteBuilder EndpointRouteBuilder { get; }
}
