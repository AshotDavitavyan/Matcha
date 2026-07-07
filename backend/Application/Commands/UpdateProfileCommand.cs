using Application.Dtos.UserDtos;
using Domain.Entities;
using Domain.Repositories;

using MediatR;

namespace Application.Commands;

public record UpdateProfileCommand(int Id, UpdateUserProfileDto UserProfileDto) : IRequest;

public class UpdateProfileCommandHandler(IUserProfileRepository userProfileRepository)
	: IRequestHandler<UpdateProfileCommand>
{
	public async Task Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
	{
		var profile = new UserProfile
		{
			Id = command.Id,
			FirstName = command.UserProfileDto.FirstName,
			LastName = command.UserProfileDto.LastName,
			Email = command.UserProfileDto.Email,
			Biography = command.UserProfileDto.Biography,
			Gender = command.UserProfileDto.Gender,
			SexualPreference = command.UserProfileDto.SexualPreference,
			Tags = command.UserProfileDto.Tags
		};
		await userProfileRepository.UpdateProfile(profile);
	}
}