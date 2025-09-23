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
    public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] GetUserByIdQuery query)
    {
        return Ok(await mediator.Send(query));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserCommand command)
    {
        return Ok(await mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] DeleteUserCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }
}