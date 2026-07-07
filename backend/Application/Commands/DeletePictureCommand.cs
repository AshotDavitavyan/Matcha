using Application.Interfaces;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record DeletePictureCommand(int UserId, int PictureId) : IRequest;

public class DeletePictureCommandHandler(IUserPictureRepository userRepository, IPictureStorage storage) : IRequestHandler<DeletePictureCommand>
{
	public async Task Handle(DeletePictureCommand request, CancellationToken cancellationToken)
	{
		string url = await userRepository.RemovePicture(request.UserId, request.PictureId);
		await storage.Delete(url, cancellationToken);
	}
}
