using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record DeleteUserCommand(int id) : IRequest;

public class DeleteUserCommandHandler (IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
	public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
	{
		await userRepository.Delete(request.id);
	}
}