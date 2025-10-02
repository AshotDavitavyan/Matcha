namespace Application.Dtos.UserDtos;

public record UpdateUserDto
{
	public string Username { get; set; }
	public string Email { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
}