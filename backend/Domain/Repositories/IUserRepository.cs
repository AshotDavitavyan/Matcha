using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<int> Create(User user);
    Task<IList<User>> GetAll();
    Task<User?> GetById(int id);
    Task<User> Update(User user);
    Task UpdatePassword(int userId, string passwordHash);
    Task<User?> GetByUsername(string username);
    Task<User?> GetByRefreshToken(string requestRefreshToken);
    Task SaveRefreshToken(int userId, string refreshToken, DateTime expiry);
}
