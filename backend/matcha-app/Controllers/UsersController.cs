using Application.Commands;
using Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController (IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var command = new CreateUserCommand(
            dto.Username,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.Password);
        var id = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllUsersQuery();
        return Ok(await mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] GetUserByIdQuery query)
    {
        var user = await mediator.Send(query);
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var command = new UpdateUserCommand(id, dto);
        return Ok(await mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] DeleteUserCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }
}