using System.Runtime.InteropServices;
using Domain.Entities;

namespace Domain.Repositories;

public interface IUserRepository
{
    Task<int> Create(User user);
    // Task<User> Get(int id);
    // Task<IEnumerable<User>> GetAll();
    // Task Update(User user);
    // Task Delete(int id);
}