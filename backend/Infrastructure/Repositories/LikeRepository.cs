using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class LikeRepository(DbConnectionFactory factory) : ILikeRepository
{
	public async Task LikeUser(int likerId, int likedId, CancellationToken token)
	{
		await using NpgsqlConnection conn = factory.CreateConnection();
		await conn.OpenAsync(token);
		await using NpgsqlCommand likeUser = new NpgsqlCommand("INSERT INTO user_likes (liker_id, liked_id) VALUES (@likerId, @likedId) ON CONFLICT (liker_id, liked_id) DO NOTHING;", conn);
		likeUser.Parameters.AddWithValue("@likerId", likerId);
		likeUser.Parameters.AddWithValue("@likedId", likedId);
		await likeUser.ExecuteNonQueryAsync(token);
	}

	public async Task UnlikeUser(int likerId, int likedId, CancellationToken token)
	{
		await using NpgsqlConnection conn = factory.CreateConnection();
		await conn.OpenAsync(token);
		await using NpgsqlCommand unlikeUser = new NpgsqlCommand("DELETE FROM user_likes WHERE liker_id = @likerId AND liked_id = @likedId", conn);
		unlikeUser.Parameters.AddWithValue("@likerId", likerId);
		unlikeUser.Parameters.AddWithValue("@likedId", likedId);
		await unlikeUser.ExecuteNonQueryAsync(token);
	}

	public async Task<bool> HasUserLiked(int likerId, int likedId, CancellationToken token)
	{
		await using NpgsqlConnection conn = factory.CreateConnection();
		await conn.OpenAsync(token);
		await using NpgsqlCommand query = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM user_likes WHERE liker_id = @likerId AND liked_id = @likedId);", conn);
		query.Parameters.AddWithValue("@likerId", likerId);
		query.Parameters.AddWithValue("@likedId", likedId);
		return await query.ExecuteScalarAsync(token) is true;
	}
}