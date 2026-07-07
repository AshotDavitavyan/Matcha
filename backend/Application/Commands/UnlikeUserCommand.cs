using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record UnlikeUserCommand(int LikerId, int LikedId) : IRequest;

public class UnlikeUserCommandHandler(ILikeRepository likeRepository, IUserAccountRepository accountRepository) : IRequestHandler<UnlikeUserCommand>
{
	public async Task Handle(UnlikeUserCommand request, CancellationToken cancellationToken)
	{
		if (request.LikerId == request.LikedId)
		{
			throw new SelfLikeException();
		}
		if (await accountRepository.GetById(request.LikedId) == null)
		{
			throw new UserNotFoundException(request.LikedId.ToString());
		}
		await likeRepository.UnlikeUser(request.LikerId, request.LikedId);
	}
}
