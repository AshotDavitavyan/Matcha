using System.Security.Claims;
using Application.Commands;
using Application.Dtos.UserDtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
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
        return Ok(await mediator.Send(query));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != id)
        {
            return Forbid();
        }
        var command = new UpdateUserCommand(id, dto);
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
    {
        string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != id)
        {
            return Forbid();
        }
        var command = new UpdatePasswordCommand(id, dto);
        await mediator.Send(command);
        return NoContent();
    }
}
