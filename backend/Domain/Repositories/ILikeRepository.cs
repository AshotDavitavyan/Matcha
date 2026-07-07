namespace Domain.Repositories;

public interface ILikeRepository
{
	Task LikeUser(int likerId, int likedId);
	Task UnlikeUser(int likerId, int likedId);
	Task<bool> HasUserLiked(int likerId, int likedId);
}