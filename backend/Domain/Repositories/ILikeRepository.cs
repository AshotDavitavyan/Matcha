namespace Domain.Repositories;

public interface ILikeRepository
{
	Task LikeUser(int likerId, int likedId, CancellationToken token);
	Task UnlikeUser(int likerId, int likedId, CancellationToken token);
	Task<bool> HasUserLiked(int likerId, int likedId, CancellationToken token);
}