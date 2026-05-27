using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<int> Create(User user);
    Task<IList<User>> GetAll();
    Task<User?> GetById(int id);
    Task UpdatePassword(int userId, string passwordHash);
    Task<User?> GetByUsername(string username);
    Task<User?> GetByRefreshToken(string requestRefreshToken);
    Task SaveRefreshToken(int userId, string refreshToken, DateTime expiry);
    // Task<List<string>> GetTagsByUserId(int userId);
    // Task SetTags(int userId, List<string> tags);
    Task<UserProfile?> GetUserProfile(int id);
    Task UpdateProfile(UserProfile profile);
    Task<List<Picture>> GetPicturesByUserId(int userId);
    Task<int> AddPicture(int userId, string url);
    Task<string> RemovePicture(int userId, int pictureId);
    Task SetProfilePicture(int userId, int pictureId);
    Task<bool> IsProfileComplete(int userId);
    Task ClearRefreshToken(int userId);
    Task LikeUser(int likerId, int likedId);
    Task UnlikeUser(int likerId, int likedId);
    Task<bool> HasUserLiked(int likerId, int likedId);
}
