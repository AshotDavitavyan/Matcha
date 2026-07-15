using System.Security.Claims;
using Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[Authorize]
[ApiController]
[Route("Users")]
public class PicturesController(IMediator mediator) : ControllerBase
{
	[HttpPost("{id}/pictures")]
	public async Task<IActionResult> AddPicture(int id, IFormFile file, CancellationToken token)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != id)
		{
			return Forbid();
		}

		await using Stream stream = file.OpenReadStream();
		var command = new AddPictureCommand(id, stream, file.FileName, file.ContentType, (int)file.Length);
		int pictureId = await mediator.Send(command, token);
		return Created($"/users/{id}/pictures/{pictureId}", pictureId);
	}

	[HttpDelete("{userId}/pictures/{pictureId}")]
	public async Task<IActionResult> DeletePicture(int userId, int pictureId, CancellationToken token)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != userId)
		{
			return Forbid();
		}

		var command = new DeletePictureCommand(userId, pictureId);
		await mediator.Send(command, token);
		return NoContent();
	}

	[HttpPut("{userId}/pictures/{pictureId}/profile")]
	public async Task<IActionResult> SetPfp(int userId, int pictureId, CancellationToken token)
	{
		string? userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdClaim, out int authenticatedUserId) || authenticatedUserId != userId)
		{
			return Forbid();
		}

		var command = new SetProfilePictureCommand(userId, pictureId);
		await mediator.Send(command, token);
		return NoContent();
	}

}