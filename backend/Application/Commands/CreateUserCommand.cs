using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record CreateUserCommand(User user) : IRequest<User>;

public class CreateUserCommandHandler (IUserRepository userRepository) : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.user.Username))
            throw new ArgumentException("Username cannot be null or empty", nameof(request.user.Username));
        var user = new User
        {
            Username = request.user.Username,
            Email = request.user.Email,
            Password = request.user.Password
        };

        var id = await userRepository.Create(user);
        user.Id = id;
        
        return user;
    }
}