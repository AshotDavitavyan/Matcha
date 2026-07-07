using Domain.Entities;

namespace Domain.Repositories;

public interface IUserProfileRepository
{
	Task<UserProfile?> GetUserProfile(int id);
	Task UpdateProfile(UserProfile profile);
	Task<bool> IsProfileComplete(int userId);
}