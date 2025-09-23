using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UpdateUserCommand(User user) : IRequest<User>;

public class UpdateUserCommandHandler (IUserRepository userRepository) : IRequestHandler<UpdateUserCommand, User>
{
	public async Task<User> Handle(UpdateUserCommand request, CancellationToken token)
	{
		User updUser = await userRepository.Update(request.user);
		return updUser;
	}
}