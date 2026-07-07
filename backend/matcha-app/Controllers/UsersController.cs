using System.Security.Claims;
using Application.Commands;
using Application.Dtos;
using Application.Dtos.UserDtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController (IMediator mediator) : ControllerBase
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
