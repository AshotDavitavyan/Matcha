using System.Security.Claims;
using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[Authorize]
[ApiController]
[Route("Users")]
public class LikesController(IMediator mediator) : ControllerBase
{
	[Authorize(Policy = "CompleteProfile")]
	[HttpPost("{likerId}/likes/{likedId}")]
	public async Task<IActionResult> LikeUser(int likerId, int likedId, CancellationToken token)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != likerId)
		{
			return Forbid();
		}

		LikeUserCommand command = new LikeUserCommand(likerId, likedId);
		bool matched = await mediator.Send(command, token);
		return Ok(new { matched });
	}

	[HttpDelete("{likerId}/likes/{likedId}")]
	public async Task<IActionResult> UnlikeUser(int likerId, int likedId, CancellationToken token)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != likerId)
		{
			return Forbid();
		}

		UnlikeUserCommand command = new UnlikeUserCommand(likerId, likedId);
		await mediator.Send(command, token);
		return NoContent();
	}
}