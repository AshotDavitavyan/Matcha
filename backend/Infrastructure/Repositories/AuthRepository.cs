using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class AuthRepository(DbConnectionFactory factory) : IAuthRepository
{
    public async Task SaveRefreshToken(int userId, string refreshToken, DateTime expiry, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "UPDATE users " +
            "SET refresh_token = @refreshToken, refresh_token_expiry = @refreshTokenExpiry " +
            "WHERE id = @id;",
            conn);
        sql.Parameters.AddWithValue("@id", userId);
        sql.Parameters.AddWithValue("@refreshToken", refreshToken);
        sql.Parameters.AddWithValue("@refreshTokenExpiry", expiry);
        await sql.ExecuteNonQueryAsync(token);
    }

    public async Task<User?> GetByRefreshToken(string requestRefreshToken, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email, password, " +
            "refresh_token, refresh_token_expiry " +
            "FROM users WHERE refresh_token = @refreshToken;",
            conn);
        sql.Parameters.AddWithValue("@refreshToken", requestRefreshToken);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync(token);
        if (await reader.ReadAsync(token))
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                Password = reader.GetString(5),
                RefreshToken = reader.GetString(6),
                RefreshTokenExpiry = reader.GetDateTime(7)
            };
        }

        return null;
    }

    public async Task ClearRefreshToken(int userId, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var command = new NpgsqlCommand(
            "UPDATE users SET refresh_token = NULL, refresh_token_expiry = NULL WHERE id = @userId;",
            conn);
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync(token);
    }
}
