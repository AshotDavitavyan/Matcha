using Domain.Entities.Authentication;

namespace Domain.Repositories;

public interface IEmailVerificationTokenRepository
{
	Task Save(EmailVerificationToken verificationToken, CancellationToken cancellationToken);
	Task<EmailVerificationToken> GetByTokenHash(string tokenHash, CancellationToken cancellationToken);
	Task DeleteByUserId(int userId, CancellationToken cancellationToken);
}