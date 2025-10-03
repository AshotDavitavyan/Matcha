using Application.Dtos;
using Application.Dtos.UserDtos;
using Application.Interfaces;
using Application.Security;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UpdatePasswordCommand(
	int Id,
	UpdatePasswordDto Dto
	) : IRequest;

public class UpdatePasswordCommandHandler(IUserRepository userRepository) : IRequestHandler<UpdatePasswordCommand>
{
	public async Task Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
	{
		IPasswordHasher hasher = new PasswordHasher();
		string hashedNew = hasher.HashPassword(request.Dto.NewPassword);
		User? user = await userRepository.GetById(request.Id);
		if (user == null)
			throw new Exception("User does not exist.");
		if (!hasher.VerifyPassword(request.Dto.CurrentPassword, user.Password))
			throw new Exception("The current password is incorrect.");
		if (hasher.VerifyPassword(request.Dto.NewPassword, user.Password))
			throw new Exception("The new password cannot be the same as the current password.");
		await userRepository.UpdatePassword(request.Id, hashedNew);
	}
}