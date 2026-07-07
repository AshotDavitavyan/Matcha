using Domain.Entities;

namespace Domain.Repositories;

public interface IUserAccountRepository
{
	Task<int> Create(User user);
	Task<IList<User>> GetAll();
	Task<User?> GetById(int id);
	Task UpdatePassword(int userId, string passwordHash);
	Task<User?> GetByUsername(string username);
}