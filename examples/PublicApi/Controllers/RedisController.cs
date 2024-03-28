using Messages;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RedisController(IRedisMediator mediator) : ControllerBase
{
    [HttpGet("notification")]
    public async Task GetNotification()
    {
        var number = Random.Shared.Next(1, 3);
        await mediator.Publish(new HelloQueueNotification("Group" + number));
    }
}
