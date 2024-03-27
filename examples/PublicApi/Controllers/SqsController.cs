// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Messages;
using Microsoft.AspNetCore.Mvc;

namespace PublicApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SqsController(IAwsSqsMediator mediator) : ControllerBase
{
    [HttpGet("notification")]
    public async Task GetNotification()
    {
        await mediator.Publish(new HelloQueueNotification("HELLO WORLD"));
    }
}
