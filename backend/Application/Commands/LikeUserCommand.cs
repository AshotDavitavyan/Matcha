using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record LikeUserCommand(int LikerId, int LikedId) : IRequest<bool>;

public class LikeUserCommandHandler(ILikeRepository likeRepository, IUserAccountRepository userAccountRepository) : IRequestHandler<LikeUserCommand, bool>
{
	public async Task<bool> Handle(LikeUserCommand request, CancellationToken cancellationToken)
	{
		if (request.LikerId == request.LikedId)
		{
			throw new SelfLikeException();
		}

		if (await userAccountRepository.GetById(request.LikedId) == null)
		{
			throw new UserNotFoundException(request.LikedId.ToString());
		}

		await likeRepository.LikeUser(request.LikerId, request.LikedId);
		return await likeRepository.HasUserLiked(request.LikedId, request.LikerId);
	}
}
