using Domain.Entities;

namespace Domain.Repositories;

public interface IUserPictureRepository
{
	Task<int> AddPicture(int userId, string url,  CancellationToken token);
	Task<string> RemovePicture(int userId, int pictureId, CancellationToken token);
	Task<List<Picture>> GetPicturesByUserId(int userId, CancellationToken token);
	Task SetProfilePicture(int userId, int pictureId, CancellationToken token);
}