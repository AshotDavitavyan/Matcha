using Application.Interfaces;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record DeletePictureCommand(int UserId, int PictureId) : IRequest;

public class DeletePictureCommandHandler(IUserRepository repository, IPictureStorage storage) : IRequestHandler<DeletePictureCommand>
{
	public async Task Handle(DeletePictureCommand request, CancellationToken cancellationToken)
	{
		string url = await repository.RemovePicture(request.UserId, request.PictureId);
		await storage.Delete(url, cancellationToken);
	}
}
