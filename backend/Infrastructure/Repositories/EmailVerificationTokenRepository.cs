using Domain.Entities.Authentication;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class EmailVerificationTokenRepository(DbConnectionFactory factory) : IEmailVerificationTokenRepository
{
	public async Task Save(EmailVerificationToken verificationToken, CancellationToken cancellationToken)
	{
		NpgsqlConnection connection = factory.CreateConnection();
		await connection.OpenAsync(cancellationToken);
		await using var sql = new NpgsqlCommand("INSERT INTO email_verification_tokens VALUES(@userId, @tokenHash, @expires_at);", connection);
		sql.Parameters.AddWithValue("@userId", verificationToken.UserId);
		sql.Parameters.AddWithValue("@tokenHash", verificationToken.TokenHash);
		sql.Parameters.AddWithValue("@expires_at", verificationToken.ExpiresAt);
		await sql.ExecuteNonQueryAsync(cancellationToken);
	}

	public async Task<EmailVerificationToken> GetByTokenHash(string tokenHash, CancellationToken cancellationToken)
	{
		NpgsqlConnection connection = factory.CreateConnection();
		EmailVerificationToken? verificationToken = null;
		await connection.OpenAsync(cancellationToken);
		await using var sql = new NpgsqlCommand("SELECT * FROM email_verification_tokens WHERE token_hash = @tokenHash;", connection);
		sql.Parameters.AddWithValue("@tokenHash", tokenHash);
		var reader = await sql.ExecuteReaderAsync(cancellationToken);
		if (await reader.ReadAsync(cancellationToken))
		{
			
		}
	}

	public Task DeleteByUserId(int userId, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}