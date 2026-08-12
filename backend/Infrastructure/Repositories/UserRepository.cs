using Domain.Entities.Users;
using Domain.Enums;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository(DbConnectionFactory factory) : IUserRepository
{
    public async Task UpdateUser(User profile, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using NpgsqlTransaction transaction = await conn.BeginTransactionAsync(token);

        await using var updateUser = new NpgsqlCommand(
            "UPDATE users " +
            "SET firstname = @firstname, lastname = @lastname, email = @email, " +
            "gender = @gender::gender_type, " +
            "sexual_preferences = @sexual_preferences::sexual_preference_type, " +
            "biography = @biography " +
            "WHERE id = @id;",
            conn,
            transaction);
        updateUser.Parameters.AddWithValue("@firstname", profile.FirstName);
        updateUser.Parameters.AddWithValue("@lastname", profile.LastName);
        updateUser.Parameters.AddWithValue("@email", profile.Email);
        updateUser.Parameters.AddWithValue(
            "@gender",
            profile.Gender.HasValue ? profile.Gender.Value.ToString().ToLowerInvariant() : DBNull.Value);
        updateUser.Parameters.AddWithValue(
            "@sexual_preferences",
            profile.SexualPreference.HasValue
                ? profile.SexualPreference.Value.ToString().ToLowerInvariant()
                : DBNull.Value);
        updateUser.Parameters.AddWithValue("@biography", profile.Biography ?? (object)DBNull.Value);
        updateUser.Parameters.AddWithValue("@id", profile.Id);

        try
        {
            await updateUser.ExecuteNonQueryAsync(token);
            if (profile.Tags.Count == 0)
            {
                await using var deleteAllUserTags =
                    new NpgsqlCommand("DELETE FROM user_tags WHERE user_id = @userId", conn, transaction);
                deleteAllUserTags.Parameters.AddWithValue("@userId", profile.Id);
                await deleteAllUserTags.ExecuteNonQueryAsync(token);
            }
            else
            {
                List<string> valueClauses = profile.Tags.Select((_, i) => $"(@name{i})").ToList();
                await using var upsertTags = new NpgsqlCommand(
                    $"INSERT INTO tags (name) VALUES {string.Join(", ", valueClauses)} " +
                    "ON CONFLICT (name) DO NOTHING",
                    conn,
                    transaction);
                for (int i = 0; i < valueClauses.Count; i++)
                {
                    upsertTags.Parameters.AddWithValue($"@name{i}", profile.Tags[i]);
                }

                await using var deleteUserTags = new NpgsqlCommand(
                    "DELETE FROM user_tags " +
                    "WHERE user_id = @userId " +
                    "AND tag_id NOT IN (SELECT id FROM tags WHERE name = ANY (@names))",
                    conn,
                    transaction);
                deleteUserTags.Parameters.AddWithValue("@userId", profile.Id);
                deleteUserTags.Parameters.AddWithValue("@names", profile.Tags.ToArray());

                await using var insertUserTags = new NpgsqlCommand(
                    "INSERT INTO user_tags (user_id, tag_id) " +
                    "SELECT @user_id, id FROM tags WHERE name = ANY(@tags) " +
                    "ON CONFLICT (user_id, tag_id) DO NOTHING",
                    conn,
                    transaction);
                insertUserTags.Parameters.AddWithValue("@user_id", profile.Id);
                insertUserTags.Parameters.AddWithValue("@tags", profile.Tags.ToArray());

                await upsertTags.ExecuteNonQueryAsync(token);
                await deleteUserTags.ExecuteNonQueryAsync(token);
                await insertUserTags.ExecuteNonQueryAsync(token);
            }
            await transaction.CommitAsync(token);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<int> Create(User user, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "INSERT INTO users (Username, FirstName, LastName, Email, Password) " +
            "VALUES (@Username, @FirstName, @LastName, @Email, @Password) " +
            "RETURNING Id;",
            conn);
        sql.Parameters.AddWithValue("@Username", user.Username);
        sql.Parameters.AddWithValue("@FirstName", user.FirstName);
        sql.Parameters.AddWithValue("@LastName", user.LastName);
        sql.Parameters.AddWithValue("@Email", user.Email);
        sql.Parameters.AddWithValue("@Password", user.PasswordHash);
        return (int)await sql.ExecuteScalarAsync(token);
    }

    public async Task<IList<User>> GetAll(CancellationToken token)
    {
        List<User> users = new List<User>();
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email FROM users;", conn);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token))
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
            });
        }
        return users;
    }

    public async Task<User?> GetById(int id, CancellationToken token)
    {
        User? user = null;
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "SELECT u.id, u.username, u.firstname, u.lastname, u.email, u.password, u.biography, " +
            "u.gender, u.sexual_preferences, array_agg(t.name) " +
            "FILTER (WHERE t.name IS NOT NULL) " +
            "FROM users u " +
            "LEFT JOIN user_tags ut ON ut.user_id = u.id " +
            "LEFT JOIN tags t ON t.id = ut.tag_id " +
            "WHERE u.id = @id " +
            "GROUP BY u.id;",
            conn);
        sql.Parameters.AddWithValue("@id", id);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync(token);
        if (await reader.ReadAsync(token))
        {
            user = new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                PasswordHash = reader.GetString(5),
                Biography = reader.IsDBNull(6) ? null : reader.GetString(6),
                Gender = reader.IsDBNull(7) ? null : Enum.Parse<Gender>(reader.GetString(7), true),
                SexualPreference = reader.IsDBNull(8) ? null : Enum.Parse<SexualPreference>(reader.GetString(8), true),
                Tags = reader.IsDBNull(9) ? new List<string>() : reader.GetFieldValue<string[]>(9).ToList(),
            };
        }

        return user;
    }

    public async Task UpdatePassword(int requestId, string hashedNew, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "UPDATE users SET Password = @Password WHERE Id = @Id;", conn);
        sql.Parameters.AddWithValue("@Password", hashedNew);
        sql.Parameters.AddWithValue("@Id", requestId);
        await sql.ExecuteNonQueryAsync(token);
    }

    public async Task<User?> GetByUsername(string username, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email, password FROM users WHERE username = @username;", conn);
        sql.Parameters.AddWithValue("@username", username);
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
                PasswordHash = reader.GetString(5)
            };
        }
        return null;
    }

    public async Task<bool> IsProfileComplete(int userId, CancellationToken token)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync(token);
        await using var command = new NpgsqlCommand(
            "SELECT EXISTS (" +
            "SELECT 1 FROM users u " +
            "WHERE u.id = @userId " +
            "AND u.biography IS NOT NULL " +
            "AND btrim(u.biography) <> '' " +
            "AND u.gender IS NOT NULL " +
            "AND u.sexual_preferences IS NOT NULL " +
            "AND EXISTS( " +
            "SELECT 1 FROM user_pictures p WHERE p.user_id = u.id))",
            conn);
        command.Parameters.AddWithValue("@userId", userId);
        bool? res = (bool?)await command.ExecuteScalarAsync(token);
        return res ?? false;
    }
}
