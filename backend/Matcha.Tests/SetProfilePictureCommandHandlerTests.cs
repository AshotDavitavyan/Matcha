using Application.Commands;
using Domain.Exceptions;
using Domain.Repositories;
using NSubstitute;

namespace Matcha.Tests;

public class SetProfilePictureCommandHandlerTests
{
	[Fact]
	public async Task Handle_ExistingPicture_CallsRepository()
	{
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new SetProfilePictureCommandHandler(repository);
		var command = new SetProfilePictureCommand(1, 10);

		await handler.Handle(command, CancellationToken.None);

		await repository.Received(1).SetProfilePicture(command.UserId, command.PictureId, CancellationToken.None);
	}

	[Fact]
	public async Task Handle_PictureNotFound_Rethrows()
	{
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new SetProfilePictureCommandHandler(repository);
		var command = new SetProfilePictureCommand(1, 10);

		repository.SetProfilePicture(command.UserId, command.PictureId, CancellationToken.None)
			.Returns<Task>(_ => throw new PictureNotFoundException());

		await Assert.ThrowsAsync<PictureNotFoundException>(() =>
			handler.Handle(command, CancellationToken.None));
	}
}
