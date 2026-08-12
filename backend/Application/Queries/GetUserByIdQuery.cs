using Application.Dtos.UserDtos;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public record GetUserByIdQuery(int id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler(IUserRepository userRepository, IUserPictureRepository pictureRepository) : IRequestHandler<GetUserByIdQuery, UserDto>
{
	public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
	{
		User user = await userRepository.GetById(query.id, cancellationToken) ?? throw new UserNotFoundException(query.id.ToString());

		return new UserDto
		{
			Id = user.Id,
			Username = user.Username,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Biography = user.Biography,
			SexualPreference = user.SexualPreference,
			Gender = user.Gender,
			Tags = user.Tags,
			Pictures = (await pictureRepository.GetPicturesByUserId(user.Id, cancellationToken)).Select(picture => new PictureDto
			{
				Id = picture.Id,
				Url = picture.Url,
				IsPfp = picture.IsPfp
			}).ToList()
		};
	}
}
