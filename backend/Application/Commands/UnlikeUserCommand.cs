using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UnlikeUserCommand(int LikerId, int LikedId) : IRequest;

public class UnlikeUserCommandHandler(IUserRepository userRepository) : IRequestHandler<UnlikeUserCommand>
{
	public async Task Handle(UnlikeUserCommand request, CancellationToken cancellationToken)
	{
		if (request.LikerId == request.LikedId)
		{
			throw new SelfLikeException();
		}
		if (await userRepository.GetById(request.LikedId) == null)
		{
			throw new UserNotFoundException(request.LikedId.ToString());
		}
		await userRepository.UnlikeUser(request.LikerId, request.LikedId);
	}
}
