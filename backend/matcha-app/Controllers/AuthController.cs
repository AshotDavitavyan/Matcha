using Application.Commands;
using Application.Dtos.AuthDtos;
using MediatR;
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
}