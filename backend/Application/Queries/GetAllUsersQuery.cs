using Application.Dtos.UserDtos;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public record GetAllUsersQuery() : IRequest<IList<UserSummaryDto>>;
public class GetAllUsersCommandHandler (IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, IList<UserSummaryDto>>
{
	public async Task<IList<UserSummaryDto>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
	{
		var users = await userRepository.GetAll();
		return users.Select(u => new UserSummaryDto
		{
			Id = u.Id,
			Username = u.Username
		}).ToList();
	}
}