using MediatR.Remote;
using Messages;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IRemoteMediator _remoteMediator;

    public TestController(IRemoteMediator remoteMediator)
    {
        _remoteMediator = remoteMediator;
    }

    [HttpGet("request")]
    public Task<HelloResponse> GetRequest()
    {
        return _remoteMediator.Send(new HelloRemoteRequest("HELLO REQUEST"));
    }

    [HttpGet("notification")]
    public Task GetNotification()
    {
        return _remoteMediator.Publish(new HelloRemoteNotification("HELLO NOTIFICATION"));
    }

    [HttpGet("stream")]
    public IAsyncEnumerable<HelloRemoteStreamResponse> GetStream()
    {
        return _remoteMediator.CreateStream(new HelloRemoteStreamRequest("HELLO STREAM"));
    }
}
