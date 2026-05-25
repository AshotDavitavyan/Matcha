using Application.Dtos.UserDtos;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public record GetUserProfileQuery(int Id) : IRequest<UserProfileDto?>;

public class GetUserProfileQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
	public async Task<UserProfileDto?> Handle(GetUserProfileQuery query, CancellationToken cancellationToken)
	{
		var userProfile = await userRepository.GetUserProfile(query.Id) ?? throw new UserNotFoundException(query.Id.ToString());
		return new UserProfileDto
		{
			Id = userProfile.Id,
			Username = userProfile.Username,
			FirstName = userProfile.FirstName,
			LastName = userProfile.LastName,
			Email = userProfile.Email,
			Biography = userProfile.Biography,
			Gender = userProfile.Gender,
			SexualPreference = userProfile.SexualPreference,
			Tags = userProfile.Tags,
			Pictures = userProfile.Pictures.Select(p => new PictureDto{Id = p.Id, Url = p.Url, IsPfp = p.IsPfp}).ToList()
		};
	}
}