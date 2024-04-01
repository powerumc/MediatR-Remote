using Messages;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
[Route("[controller]")]
public class KafkaController(IKafkaMediator mediator) : ControllerBase
{
    [HttpGet("notification")]
    public async Task GetNotification()
    {
        await mediator.Publish(new HelloQueueNotification("HELLO WORLD"));
    }
}
