using Domain.Entities;

namespace Domain.Repositories;

public interface IUserAccountRepository
{
	Task<int> Create(User user, CancellationToken token);
	Task<IList<User>> GetAll(CancellationToken token);
	Task<User?> GetById(int id, CancellationToken token);
	Task UpdatePassword(int userId, string passwordHash, CancellationToken token);
	Task<User?> GetByUsername(string username, CancellationToken token);
}