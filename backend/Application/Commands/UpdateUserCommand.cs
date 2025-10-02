using Application.Dtos;
using Application.Dtos.UserDtos;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UpdateUserCommand(int Id, UpdateUserDto UserDto) : IRequest<UpdateUserDto>;

public class UpdateUserCommandHandler (IUserRepository userRepository) : IRequestHandler<UpdateUserCommand, UpdateUserDto>
{
	public async Task<UpdateUserDto> Handle(UpdateUserCommand request, CancellationToken token)
	{
		User user = new User
		{
			Id = request.Id,
			Username = request.UserDto.Username,
			Email = request.UserDto.Email,
			FirstName = request.UserDto.FirstName,
			LastName = request.UserDto.LastName,
		};
		
		User? updUser = await userRepository.Update(user);
		if (updUser == null) return null;
		
		return new  UpdateUserDto
		{
			Username = updUser.Username,
			FirstName = updUser.FirstName,
			LastName = updUser.LastName,
			Email = updUser.Email
		};
	}
}