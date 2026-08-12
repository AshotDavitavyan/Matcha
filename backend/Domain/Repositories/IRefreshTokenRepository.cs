using Domain.Entities.Authentication;

namespace Domain.Repositories;

public interface IRefreshTokenRepository
{
	Task DeleteByUserId(int userId, CancellationToken token);
	Task<RefreshToken?> GetByTokenHash(string requestRefreshToken, CancellationToken token);
	Task Save(RefreshToken refreshToken, CancellationToken cancellationToken);
}