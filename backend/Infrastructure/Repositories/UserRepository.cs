using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository(DbConnectionFactory factory) : IUserRepository
{
    public async Task<int> Create(User user)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "INSERT INTO users (Username, FirstName, LastName, Email, Password)" +
                    "VALUES (@Username, @FirstName, @LastName, @Email, @Password) " + 
                    "RETURNING Id;", conn);
        sql.Parameters.AddWithValue("@Username", user.Username);
        sql.Parameters.AddWithValue("@FirstName", user.FirstName);
        sql.Parameters.AddWithValue("@LastName", user.LastName);
        sql.Parameters.AddWithValue("@Email", user.Email);
        sql.Parameters.AddWithValue("@Password", user.Password);
        
        return (int)await sql.ExecuteScalarAsync();
    }

    public async Task<IList<User>> GetAll()
    {
        List<User> users = new List<User>();
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email FROM users;", conn);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync();
        while (await reader.ReadAsync())
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

    public async Task<User?> GetById(int id)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email, password FROM users WHERE Id = @Id;", conn);
        sql.Parameters.AddWithValue("@Id", id);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                Password = reader.GetString(5)
            };
        }
        return null;
    }
    
    public async Task UpdatePassword(int requestId, string hashedNew)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "UPDATE users SET Password = @Password WHERE Id = @Id;", conn);
        sql.Parameters.AddWithValue("@Password", hashedNew);
        sql.Parameters.AddWithValue("@Id", requestId);
        await sql.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetByUsername(string username)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email, password FROM users WHERE username = @username;", conn);
        sql.Parameters.AddWithValue("@username", username);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                Password = reader.GetString(5)
            };
        }
        return null;
    }

    public async Task<User?> GetByRefreshToken(string requestRefreshToken)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email, password, refresh_token, refresh_token_expiry FROM users WHERE refresh_token = @refreshToken;",
            conn);
        sql.Parameters.AddWithValue("@refreshToken", requestRefreshToken);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync();
        if (await reader.ReadAsync())
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

public async Task<UserProfile?> GetUserProfile(int id)
{
    UserProfile? profile = null;
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand("SELECT u.id, u.username, u.firstname, u.lastname, u.email, u.biography, u.gender, u.sexual_preferences, array_agg(t.name) " +
                                                "FILTER (WHERE t.name IS NOT NULL) " +
                                                "FROM users u " +
                                                "LEFT JOIN user_tags ut ON ut.user_id = u.id " +
                                                "LEFT JOIN tags t ON t.id = ut.tag_id " +
                                                "WHERE u.id = @id " +
                                                "GROUP BY u.id;", conn);
        sql.Parameters.AddWithValue("@id", id);
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            profile =  new UserProfile
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                Biography = reader.IsDBNull(5) ? null : reader.GetString(5),
                Gender = reader.IsDBNull(6) ? null : Enum.Parse<Gender>(reader.GetString(6), true),
                SexualPreference = reader.IsDBNull(7) ? null : Enum.Parse<SexualPreference>(reader.GetString(7), true),
                Tags = reader.IsDBNull(8) ? new List<string>() : reader.GetFieldValue<string[]>(8).ToList(),
            };
            profile.Pictures = await GetPicturesByUserId(profile.Id);
        }

        return profile;
    }

    public async Task SaveRefreshToken(int userId, string refreshToken, DateTime expiry)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql =
            new NpgsqlCommand(
                "UPDATE users SET refresh_token = @refreshToken, refresh_token_expiry = @refreshTokenExpiry WHERE id = @id;" ,
                conn);
        sql.Parameters.AddWithValue("@id", userId);
        sql.Parameters.AddWithValue("@refreshToken", refreshToken);
        sql.Parameters.AddWithValue("@refreshTokenExpiry", expiry);
        await sql.ExecuteNonQueryAsync();
    }


    public async Task UpdateProfile(UserProfile profile)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using NpgsqlTransaction transaction = await conn.BeginTransactionAsync();
        
        await using var updateUser = new NpgsqlCommand("UPDATE users SET firstname = @firstname, lastname = @lastname, email = @email, gender = @gender::gender_type, sexual_preferences = @sexual_preferences::sexual_preference_type, biography = @biography WHERE id = @id;", conn, transaction);
        updateUser.Parameters.AddWithValue("@firstname", profile.FirstName);
        updateUser.Parameters.AddWithValue("@lastname", profile.LastName);
        updateUser.Parameters.AddWithValue("@email", profile.Email);
        updateUser.Parameters.AddWithValue("@gender", profile.Gender.HasValue ? profile.Gender.Value.ToString().ToLowerInvariant() : DBNull.Value);
        updateUser.Parameters.AddWithValue("@sexual_preferences", profile.SexualPreference.HasValue ? profile.SexualPreference.Value.ToString().ToLowerInvariant() :  DBNull.Value);
        updateUser.Parameters.AddWithValue("@biography", profile.Biography ?? (object)DBNull.Value);
        updateUser.Parameters.AddWithValue("@id", profile.Id);
        
        List<string> valueClauses = profile.Tags.Select((_, i) => $"(@name{i})").ToList();
        await using var upsertTags = new NpgsqlCommand($"INSERT INTO tags (name) VALUES {string.Join(", ", valueClauses)} " +
                                                                         $"ON CONFLICT (name) DO NOTHING", conn, transaction);
        for (int i = 0; i < valueClauses.Count; i++)
            upsertTags.Parameters.AddWithValue($"@name{i}", profile.Tags[i]);
        
        await using var deleteUserTags = new NpgsqlCommand($"DELETE FROM user_tags " +
                                                                             $"WHERE user_id = @userId " +
                                                                             $"AND tag_id NOT IN (SELECT id FROM tags WHERE name = ANY (@names))", conn, transaction);
        deleteUserTags.Parameters.AddWithValue("@userId", profile.Id);
        deleteUserTags.Parameters.AddWithValue("@names", profile.Tags.ToArray());
        
        await using var insertUserTags = new NpgsqlCommand(
            $"INSERT INTO user_tags (user_id, tag_id) " +
                    $"SELECT @user_id, id FROM tags WHERE name = ANY(@tags) ON CONFLICT (user_id, tag_id) DO NOTHING", conn, transaction);
        insertUserTags.Parameters.AddWithValue("@user_id", profile.Id);
        insertUserTags.Parameters.AddWithValue("@tags", profile.Tags.ToArray());
        try
        {
            await updateUser.ExecuteNonQueryAsync();
            await upsertTags.ExecuteNonQueryAsync();
            await deleteUserTags.ExecuteNonQueryAsync();
            await insertUserTags.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // public async Task<List<string>> GetTagsByUserId(int userId)
    // {
    //     await using NpgsqlConnection conn = factory.CreateConnection();
    //     await conn.OpenAsync();
    //     await using NpgsqlCommand sql =
    //         new NpgsqlCommand(
    //                 "SELECT t.name FROM tags t INNER JOIN user_tags ut ON ut.tag_id = t.id WHERE ut.user_id = @userId;", conn
    //             );
    //     sql.Parameters.AddWithValue("@userId", userId);
    //     await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync(); 
    //     List<string> tags = new List<string>();
    //     while (await reader.ReadAsync()) 
    //     {
    //         tags.Add(reader.GetString(0));
    //     }
    //     return tags;
    // }
    
    // public async Task SetTags(int userId, List<string> tags)
    // {
    //     await using NpgsqlConnection conn = factory.CreateConnection();
    //     await conn.OpenAsync();
    //     await using NpgsqlTransaction transaction = await conn.BeginTransactionAsync();
    //     List<string> valuesClauses = tags.Select((_, i) => $"(@name{i})").ToList();
    //     await using NpgsqlCommand upsertTags = new NpgsqlCommand(
    //         $"INSERT INTO tags (name) VALUES {string.Join(", ", valuesClauses)} ON CONFLICT (name) DO NOTHING;", conn, transaction);
    //     for (int i = 0; i < tags.Count; i++)
    //         upsertTags.Parameters.AddWithValue($"@name{i}", tags[i]);
    //
    //     NpgsqlCommand syncUserTags = new NpgsqlCommand(
    //         "DELETE FROM user_tags WHERE user_id = @userId AND tag_id NOT IN (SELECT id FROM tags WHERE name = @name)", conn, transaction);
    //     syncUserTags.Parameters.AddWithValue("@userId", userId);
    //     
    //     NpgsqlCommand insertUserTags = new NpgsqlCommand(
    //         "INSERT INTO user_tags (user_id, tag_id) " +
    //         "SELECT @userId, id FROM tags WHERE name = ANY(@names) " +
    //         "ON CONFLICT (user_id, tag_id) DO NOTHING", conn);
    //     insertUserTags.Parameters.AddWithValue("@userId", userId);
    //     
    //     try
    //     {
    //         await upsertTags.ExecuteNonQueryAsync();
    //         await syncUserTags.ExecuteNonQueryAsync();
    //         await insertUserTags.ExecuteNonQueryAsync();
    //         await transaction.CommitAsync();
    //     }
    //     catch
    //     {
    //         await transaction.RollbackAsync();
    //         throw;
    //     }
    // }
    
    // Might need later
    public async Task<List<Picture>> GetPicturesByUserId(int userId)
    {
        List<Picture> pictures = new List<Picture>();
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand($"SELECT * FROM user_pictures WHERE user_id = @userId", conn);
        sql.Parameters.AddWithValue("@userId", userId);
        
        await using NpgsqlDataReader reader = await sql.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            pictures.Add(new Picture
            {
                Id = (int)reader["id"],
                Url = (string)reader["url"],
                UserId = (int)reader["user_id"],
                IsPfp = (bool)reader["is_pfp"],
            });
        }
        return pictures;
    }
    
    public async Task<int> AddPicture(int userId, string url)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var getPictures = new NpgsqlCommand("SELECT COUNT(*) FROM user_pictures WHERE user_id = @userId", conn);
        getPictures.Parameters.AddWithValue("@userId", userId);
        long count = (long)await getPictures.ExecuteScalarAsync();
        if (count >= 5)
        {
            throw new PictureLimitExceededException();
        }
        await using var insertPictures = new NpgsqlCommand("INSERT INTO user_pictures (user_id, url, is_pfp) VALUES (@userId, @url, @isPfp) RETURNING id", conn);
        insertPictures.Parameters.AddWithValue("@userId", userId);
        insertPictures.Parameters.AddWithValue("@url", url);
        insertPictures.Parameters.AddWithValue("@isPfp", count == 0);
        int res = (int)(await insertPictures.ExecuteScalarAsync())!;
        return res;
    }
    
    public async Task<string> RemovePicture(int userId, int pictureId)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using NpgsqlTransaction transaction = await conn.BeginTransactionAsync();
        await using var deletePicture =
            new NpgsqlCommand("DELETE FROM user_pictures WHERE user_id = @userId AND id = @id RETURNING url, is_pfp", conn, transaction);
        deletePicture.Parameters.AddWithValue("@userId", userId);
        deletePicture.Parameters.AddWithValue("@id", pictureId);
        await using var updatePfp = new NpgsqlCommand("UPDATE user_pictures SET is_pfp = true WHERE id = (SELECT id FROM user_pictures WHERE user_id = @userId ORDER BY id LIMIT 1)", conn, transaction);
        updatePfp.Parameters.AddWithValue("@userId", userId);
        try
        {
            await using NpgsqlDataReader reader = await deletePicture.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new PictureNotFoundException();
            }
            string url = reader.GetString(0);
            bool isPfp = (bool)reader.GetBoolean(1);
            await reader.CloseAsync();
            if (isPfp)
            {
                await updatePfp.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return url;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SetProfilePicture(int userId, int pictureId)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using NpgsqlTransaction transaction = await conn.BeginTransactionAsync();
        await using var unsetPfp = new NpgsqlCommand(
            "UPDATE user_pictures SET is_pfp = false WHERE user_id = @userId AND is_pfp = true", conn, transaction);
        unsetPfp.Parameters.AddWithValue("@userId", userId);
        await using var setPfp = new NpgsqlCommand("UPDATE user_pictures SET is_pfp = true WHERE user_id = @userId AND id = @pictureId", conn, transaction);
        setPfp.Parameters.AddWithValue("@pictureId", pictureId);
        setPfp.Parameters.AddWithValue("@userId", userId);

        try
        {
            await unsetPfp.ExecuteNonQueryAsync();
            if (await setPfp.ExecuteNonQueryAsync() == 0)
            {
                throw new PictureNotFoundException();
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> IsProfileComplete(int userId)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT EXISTS (" +
                                                              "SELECT 1 FROM users u " +
                                                              "WHERE u.id = @userId " +
                                                              "AND u.biography IS NOT NULL " +
                                                              "AND btrim(u.biography) <> '' " +
                                                              "AND u.gender IS NOT NULL " +
                                                              "AND u.sexual_preferences IS NOT NULL " +
                                                              "AND EXISTS( " +
                                                              "SELECT 1 FROM user_pictures p WHERE p.user_id = u.id))", conn);
        command.Parameters.AddWithValue("@userId", userId);
        bool? res = (bool?)await command.ExecuteScalarAsync();
        return res ?? false;
    }

    public async Task ClearRefreshToken(int userId)
    {
        await using NpgsqlConnection conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var command =
            new NpgsqlCommand("UPDATE users SET refresh_token = NULL, refresh_token_expiry = NULL WHERE id = @userId;",
                conn);
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync();
    }
}
