using Domain.Entities.Users;

namespace Domain.Repositories;

public interface IUserRepository
{
	Task<int> Create(User user, CancellationToken token);
	Task<IList<User>> GetAll(CancellationToken token);
	Task<User?> GetById(int id, CancellationToken token);
	Task UpdatePassword(int userId, string passwordHash, CancellationToken token);
	Task<User?> GetByUsername(string username, CancellationToken token);
	Task UpdateUser(User user, CancellationToken token);
	Task<bool> IsProfileComplete(int userId, CancellationToken token);
}
