using Application.Dtos.UserDtos;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public record GetUserByIdQuery(int id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler(IUserAccountRepository userAccountRepository) : IRequestHandler<GetUserByIdQuery, UserDto>
{
	public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
	{
		var user = await userAccountRepository.GetById(query.id) ?? throw new UserNotFoundException(query.id.ToString());
		return new UserDto
		{
			Id = user.Id,
			Username = user.Username,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Email = user.Email,
		};
	}
}
