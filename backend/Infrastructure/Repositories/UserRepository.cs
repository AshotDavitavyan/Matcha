using System.Data.SqlClient;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory  factory)
    {
        _connectionFactory = factory;
    }
    public async Task<int> Create(User user)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var sql = new NpgsqlCommand(
            "INSERT INTO Users (Username, Email, Password)" +
                    "VALUES (@Username, @Email, @Password) " + 
                    "RETURNING Id;", conn);
        
        sql.Parameters.AddWithValue("@Username", user.Username);
        sql.Parameters.AddWithValue("@Email", user.Email);
        sql.Parameters.AddWithValue("@Password", user.Password);
        
        return (int)await sql.ExecuteScalarAsync();
    }

    // public Task<User> Get(int id)
    // {
    // }
    //
    // public Task Update(User user)
    // {
    //     
    // }
}