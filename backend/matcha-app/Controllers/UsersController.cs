using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController (IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        return Ok(await mediator.Send(command));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok();
    }
}