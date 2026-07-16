using Domain.Enums;

namespace Domain.Entities;

public class UserProfile
{
	public int Id { get; set; }
	public string Username { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string? Biography { get; set; }
	public Gender? Gender { get; set; }
	public SexualPreference? SexualPreference { get; set; }
	public List<string> Tags { get; set; } = new();
	public List<Picture> Pictures { get; set; } = new();
}
