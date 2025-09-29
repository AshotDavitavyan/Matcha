using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record GetUserByIdQuery(int id) : IRequest<User>;

public class GetUserByIdQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, User>
{
	public async Task<User> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
	{
		var user = await userRepository.GetById(query.id);
		return user;
	}
}