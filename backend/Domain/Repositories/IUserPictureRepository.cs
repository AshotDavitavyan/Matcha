using Domain.Entities;

namespace Domain.Repositories;

public interface IUserPictureRepository
{
	Task<int> AddPicture(int userId, string url);
	Task<string> RemovePicture(int userId, int pictureId);
	Task<List<Picture>> GetPicturesByUserId(int userId);
	Task SetProfilePicture(int userId, int pictureId);
}