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
[Route("Users")]
public class ProfilesController(IMediator mediator) : ControllerBase
{
	[HttpGet("{id}/profile")]
	public async Task<IActionResult> GetProfile(int id)
	{
		var query = new GetUserProfileQuery(id);
		return Ok(await mediator.Send(query));
	}

	[HttpPut("{id}/profile")]
	public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserProfileDto dto)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != id)
		{
			return Forbid();
		}
		var command = new UpdateProfileCommand(id, dto);
		await mediator.Send(command);
		return NoContent();
	}

}