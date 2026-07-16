using Application.Commands;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using NSubstitute;

namespace Matcha.Tests;

public class DeletePictureCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingPicture_RemovesDbRowAndDeletesFile()
	{
		var repository = Substitute.For<IUserPictureRepository>();
		var storage = Substitute.For<IPictureStorage>();
		var handler = new DeletePictureCommandHandler(repository, storage);
		var command = new DeletePictureCommand(1, 10);

		repository.RemovePicture(command.UserId, command.PictureId, CancellationToken.None)
			.Returns(Task.FromResult("/uploads/profile.png"));
		storage.Delete("/uploads/profile.png", CancellationToken.None)
			.Returns(Task.CompletedTask);

		await handler.Handle(command, CancellationToken.None);

		await repository.Received(1).RemovePicture(command.UserId, command.PictureId, CancellationToken.None);
		await storage.Received(1).Delete("/uploads/profile.png", CancellationToken.None);
	}

	[Fact]
	public async Task Handle_PictureNotFound_DoesNotDeleteFile()
	{
		var repository = Substitute.For<IUserPictureRepository>();
		var storage = Substitute.For<IPictureStorage>();
		var handler = new DeletePictureCommandHandler(repository, storage);
		var command = new DeletePictureCommand(1, 10);

		repository.RemovePicture(command.UserId, command.PictureId, CancellationToken.None)
			.Returns<Task<string>>(_ => throw new PictureNotFoundException());

		await Assert.ThrowsAsync<PictureNotFoundException>(() =>
			handler.Handle(command, CancellationToken.None));

		await storage.DidNotReceiveWithAnyArgs().Delete(default!, default);
	}
}
