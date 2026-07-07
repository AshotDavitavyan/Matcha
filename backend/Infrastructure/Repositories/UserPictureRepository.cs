using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserPictureRepository(DbConnectionFactory factory) : IUserPictureRepository
{
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

}
