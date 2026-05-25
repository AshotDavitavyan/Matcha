namespace Domain.Entities;

public class Picture
{
	public int Id { get; set; }
	public int UserId { get; set; }	
	public string? Url { get; set; }
	public bool IsPfp { get; set; }
}