using Domain.Entities;

namespace Domain.Repositories;

public interface IAuthRepository
{
	Task ClearRefreshToken(int userId);
	Task<User?> GetByRefreshToken(string requestRefreshToken);
	Task SaveRefreshToken(int userId, string refreshToken, DateTime expiry);
}