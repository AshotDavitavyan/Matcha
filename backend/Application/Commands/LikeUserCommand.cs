using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record LikeUserCommand(int LikerId, int LikedId) : IRequest<bool>;

public class LikeUserCommandHandler(IUserRepository userRepository) : IRequestHandler<LikeUserCommand, bool>
{
	public async Task<bool> Handle(LikeUserCommand request, CancellationToken cancellationToken)
	{
		if (request.LikerId == request.LikedId)
		{
			throw new SelfLikeException();
		}

		if (await userRepository.GetById(request.LikedId) == null)
		{
			throw new UserNotFoundException(request.LikedId.ToString());
		}

		await userRepository.LikeUser(request.LikerId, request.LikedId);
		return await userRepository.HasUserLiked(request.LikedId, request.LikerId);
	}
}