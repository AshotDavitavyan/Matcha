using Domain.Entities;

namespace Domain.Repositories;

public interface IAuthRepository
{
	Task ClearRefreshToken(int userId, CancellationToken token);
	Task<User?> GetByRefreshToken(string requestRefreshToken, CancellationToken token);
	Task SaveRefreshToken(int userId, string refreshToken, DateTime expiry, CancellationToken token);
}