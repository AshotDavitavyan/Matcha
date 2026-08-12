using Application.Dtos.UserDtos;
using Domain.Entities.Users;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UpdateUserCommand(int Id, UpdateUserDto UserDto) : IRequest;

public class UpdateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var profile = new User
        {
            Id = command.Id,
            FirstName = command.UserDto.FirstName,
            LastName = command.UserDto.LastName,
            Email = command.UserDto.Email,
            Biography = command.UserDto.Biography,
            Gender = command.UserDto.Gender,
            SexualPreference = command.UserDto.SexualPreference,
            Tags = command.UserDto.Tags
        };
        await userRepository.UpdateUser(profile, cancellationToken);
    }
}
