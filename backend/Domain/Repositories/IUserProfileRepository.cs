using Domain.Entities;

namespace Domain.Repositories;

public interface IUserProfileRepository
{
	Task<UserProfile?> GetUserProfile(int id,  CancellationToken token);
	Task UpdateProfile(UserProfile profile,  CancellationToken token);
	Task<bool> IsProfileComplete(int userId, CancellationToken token);
}