using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Database;
using Npgsql;

namespace Infrastructure.Repositories;

public class UserRepository(DbConnectionFactory factory) : IUserRepository
{
    public async Task<int> Create(User user)
    {
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "INSERT INTO users (Username, FirstName, LastName, Email, Password)" +
                    "VALUES (@Username, @FirstName, @LastName, @Email, @Password) " + 
                    "RETURNING Id;", conn);
        sql.Parameters.AddWithValue("@Username", user.Username);
        sql.Parameters.AddWithValue("@FirstName", user.FirstName);
        sql.Parameters.AddWithValue("@LastName", user.LastName);
        sql.Parameters.AddWithValue("@Email", user.Email);
        sql.Parameters.AddWithValue("@Password", user.Password);
        
        return (int)await sql.ExecuteScalarAsync();
    }

    public async Task<IList<User>> GetAll()
    {
        List<User> users = new List<User>();
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email FROM users", conn);
        await using var reader = await sql.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4)
            });
        }
        return users;
    }

    public async Task<User?> GetById(int id)
    {
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "SELECT id, username, firstname, lastname, email, password FROM users WHERE Id = @Id", conn);
        sql.Parameters.AddWithValue("@Id", id);
        using var reader = await sql.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Email = reader.GetString(4),
                Password = reader.GetString(5)
            };
        }
        return null;
    }
    
    public async Task<User> Update(User user)
    {
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "UPDATE users SET username=@username, firstname=@firstname, lastname=@lastname, email=@email WHERE Id = @Id", conn);
        sql.Parameters.AddWithValue("@username", user.Username);
        sql.Parameters.AddWithValue("@firstName", user.FirstName); 
        sql.Parameters.AddWithValue("@LastName", user.LastName);
        sql.Parameters.AddWithValue("@Email", user.Email);
        sql.Parameters.AddWithValue("@Id", user.Id);
        await sql.ExecuteNonQueryAsync();
        return user;
    }
    
    public async Task Delete(int id)
    {
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "DELETE FROM users WHERE Id = @Id", conn);
        sql.Parameters.AddWithValue("@Id", id);
        await sql.ExecuteNonQueryAsync();
    }

    public async Task UpdatePassword(int requestId, string hashedNew)
    {
        await using var conn = factory.CreateConnection();
        await conn.OpenAsync();
        await using var sql = new NpgsqlCommand(
            "UPDATE users SET Password = @Password WHERE Id = @Id", conn);
        sql.Parameters.AddWithValue("@Password", hashedNew);
        sql.Parameters.AddWithValue("@Id", requestId);
        await sql.ExecuteNonQueryAsync();
    }
}