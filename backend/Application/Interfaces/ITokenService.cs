using Domain.Entities.Users;

namespace Application.Interfaces;

public interface ITokenService
{
	string GenerateAccessToken(User user);
	string GenerateRefreshToken();
	string HashRefreshToken(string refreshToken);
}
