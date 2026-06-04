using System.Security.Claims;
using Application.Commands;
using Application.Dtos.AuthDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginDto dto)
	{
		LoginCommand command = new LoginCommand(dto.Username, dto.Password);
		var result = await mediator.Send(command);
		return Ok(result);
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
	{
		RefreshTokenCommand command = new RefreshTokenCommand(dto.RefreshToken);
		var result = await mediator.Send(command);
		return Ok(result);
	}


	[Authorize]
	[HttpPost("logout")]
	public async Task<IActionResult> Logout(CancellationToken cancellationToken)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int userId))
		{
			return Unauthorized();
		}

		var command = new LogoutCommand(userId);
		await mediator.Send(command, cancellationToken);
		return NoContent();
	}
}
