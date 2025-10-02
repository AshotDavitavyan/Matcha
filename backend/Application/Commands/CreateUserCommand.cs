using Application.Dtos;
using Application.Interfaces;
using Application.Security;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record CreateUserCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Password
    ) : IRequest<int>;

public class CreateUserCommandHandler (IUserRepository userRepository) : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        IPasswordHasher hasher = new PasswordHasher();
        string hashedPassword = hasher.HashPassword(request.Password);
        var user = new User
        {
            Username = request.Username,
            LastName = request.LastName,
            FirstName = request.FirstName,
            Email = request.Email,
            Password = hashedPassword
        };

        int id = await userRepository.Create(user);
        return id;
    }
}