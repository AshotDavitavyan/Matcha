using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record GetAllUsersQuery() : IRequest<IList<User>>;
public class GetAllUsersCommandHandler (IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, IList<User>>
{
	public async Task<IList<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
	{
		var users = await userRepository.GetAll();
		return users;
	}
}