using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using MediatR;
using Cognassist.Notification.Application;

namespace Cognassist.Notification.Api;

[ApiController]
public class NotificationController(IMediator mediator)
    : Controller
{
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Post([FromBody] IEnumerable<Domain.Notification> notifications)
    {
        var request = new BatchNotificationRequest()
        {
            Notifications = notifications
        };
        var response = await mediator.Send(request);
        return Ok(response);

    }

    [HttpPost()]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Post([FromBody] Domain.Notification notification)
    {
        var request = new BatchNotificationRequest()
        {
            Notifications = [notification]
        };
        var response = await mediator.Send(request);
        return Ok(response);
    }


}
