namespace Application.Dtos.UserDtos;

public record UpdatePasswordDto
{
	public string CurrentPassword { get; init; }
	public string NewPassword { get; init; }
}
