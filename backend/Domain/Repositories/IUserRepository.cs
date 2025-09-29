using System.Runtime.InteropServices;
using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<int> Create(User user);
    Task<IList<User>> GetAll();
    Task<User?> GetById(int id);
    Task<User> Update(User user);
    Task Delete(int id);
}