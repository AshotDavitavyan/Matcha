using Application.Dtos.UserDtos;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public record GetUserByIdQuery(int id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, UserDto>
{
	public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetById(query.id);
		return new  UserDto
		{
			Id = user.Id,
			Username = user.Username,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Email = user.Email,
		};
	}
}