using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record SetProfilePictureCommand(int UserId, int PictureId) : IRequest;

public class SetProfilePictureCommandHandler(IUserPictureRepository userRepository)
	: IRequestHandler<SetProfilePictureCommand>
{
	public async Task Handle(SetProfilePictureCommand request, CancellationToken cancellationToken)
	{
		await userRepository.SetProfilePicture(request.UserId, request.PictureId);
	}
}