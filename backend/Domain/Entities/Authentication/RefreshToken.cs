namespace Domain.Entities.Authentication;

public class RefreshToken
{
	public int UserId { get; set; }
	public string TokenHash { get; set; }
	public DateTime ExpiresAt { get; set; }
}
