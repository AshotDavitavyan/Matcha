namespace Application.Dtos;

public record UpdatePasswordDto
{
	public string CurrentPassword { get; set; }
	public string NewPassword { get; set; }
}