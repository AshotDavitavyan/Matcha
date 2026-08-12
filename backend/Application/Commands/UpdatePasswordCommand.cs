using Application.Dtos.UserDtos;
using Application.Interfaces;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UpdatePasswordCommand(int Id, UpdatePasswordDto Dto) : IRequest;

public class UpdatePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher hasher) : IRequestHandler<UpdatePasswordCommand>
{
	public async Task Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
	{
		string hashedNew = hasher.HashPassword(request.Dto.NewPassword);
		User? user = await userRepository.GetById(request.Id, cancellationToken);
		if (user == null)
			throw new UserNotFoundException(request.Id.ToString());
		if (!hasher.VerifyPassword(request.Dto.CurrentPassword, user.PasswordHash))
			throw new InvalidPasswordException();
		if (hasher.VerifyPassword(request.Dto.NewPassword, user.PasswordHash))
			throw new SamePasswordException();
		await userRepository.UpdatePassword(request.Id, hashedNew, cancellationToken);
	}
}
