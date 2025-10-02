namespace Application.Dtos;

public record UserDto
{
	public int Id { get; set; }
	public string Username { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string EmailAddress { get; set; }
}