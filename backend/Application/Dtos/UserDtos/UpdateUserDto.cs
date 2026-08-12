using Domain.Enums;

namespace Application.Dtos.UserDtos;

public record UpdateUserDto
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Biography { get; set; }
	public Gender Gender { get; set; }
	public SexualPreference SexualPreference { get; set; }
	public List<string> Tags { get; set; }
}
