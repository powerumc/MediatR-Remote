using MediatR.Remote;
using Messages;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HttpController(IRemoteMediator remoteMediator) : ControllerBase
{
    [HttpGet("request")]
    public Task<HelloResponse> GetRequest()
    {
        return remoteMediator.Send(new HelloRemoteRequest("HELLO REQUEST"));
    }

    [HttpGet("notification")]
    public Task GetNotification()
    {
        return remoteMediator.Publish(new HelloRemoteNotification("HELLO NOTIFICATION"));
    }

    [HttpGet("stream")]
    public IAsyncEnumerable<HelloRemoteStreamResponse> GetStream()
    {
        return remoteMediator.CreateStream(new HelloRemoteStreamRequest("HELLO STREAM"));
    }
}
