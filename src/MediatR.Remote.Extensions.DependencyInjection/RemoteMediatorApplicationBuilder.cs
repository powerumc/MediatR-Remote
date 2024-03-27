namespace MediatR.Remote.Extensions.DependencyInjection;

public class RemoteMediatorApplicationBuilder(WebApplication webApplication)
{
    public WebApplication WebApplication { get; } = webApplication;
}

public class RemoteMediatorEndpointRouteBuilder(IEndpointRouteBuilder endpointRouteBuilder)
{
    public IEndpointRouteBuilder EndpointRouteBuilder { get; } = endpointRouteBuilder;
}
