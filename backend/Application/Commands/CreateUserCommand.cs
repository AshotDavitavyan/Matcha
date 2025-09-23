using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record CreateUserCommand(User User) : IRequest<User>;

public class CreateUserCommandHandler (IUserRepository userRepository) : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = request.User.Username,
            LastName = request.User.LastName,
            FirstName = request.User.FirstName,
            Email = request.User.Email,
            Password = request.User.Password
        };

        var id = await userRepository.Create(user);
        user.Id = id;
        return user;
    }
}