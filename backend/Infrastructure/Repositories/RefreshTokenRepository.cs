using Domain.Entities.Authentication;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository(DbConnectionFactory factory) : IRefreshTokenRepository
{
    public async Task Save(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(cancellationToken);
        await using var sql = new NpgsqlCommand(
            "INSERT INTO refresh_tokens (user_id, token_hash, expires_at) " +
            "VALUES (@userId, @tokenHash, @expiresAt) " +
            "ON CONFLICT (user_id) " +
            "DO UPDATE SET " +
            "token_hash = EXCLUDED.token_hash, expires_at = EXCLUDED.expires_at;",
            conn);
        sql.Parameters.AddWithValue("@userId", refreshToken.UserId);
        sql.Parameters.AddWithValue("@tokenHash", refreshToken.TokenHash);
        sql.Parameters.AddWithValue("@expiresAt", refreshToken.ExpiresAt);
        await sql.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHash(string tokenHash, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "SELECT user_id, token_hash, expires_at " +
            "FROM refresh_tokens WHERE token_hash = @tokenHash;",
            conn);
        sql.Parameters.AddWithValue("@tokenHash", tokenHash);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync(token);
        if (await reader.ReadAsync(token))
        {
            return new RefreshToken
            {
                UserId = reader.GetInt32(0),
                TokenHash = reader.GetString(1),
                ExpiresAt = reader.GetDateTime(2)
            };
        }

        return null;
    }

    public async Task DeleteByUserId(int userId, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var command = new NpgsqlCommand(
            "DELETE FROM refresh_tokens WHERE user_id = @userId;",
            conn);
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync(token);
    }
}
