using Application.Commands;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using NSubstitute;

namespace Matcha.Tests;

public class AddPictureCommandHandlerTests
{
	private static AddPictureCommand ValidCommand(int byteLength = 1024)
	{
		return new AddPictureCommand(
			1,
			new MemoryStream([1, 2, 3]),
			"profile.png",
			"image/png",
			byteLength);
	}

	[Fact]
	public async Task Handle_EmptyFile_ThrowsInvalidPictureUpload()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new AddPictureCommandHandler(storage, repository);

		await Assert.ThrowsAsync<InvalidPictureUploadException>(() =>
			handler.Handle(ValidCommand(byteLength: 0), CancellationToken.None));

		await storage.DidNotReceiveWithAnyArgs().Save(default!, default!, default!, default);
		await repository.DidNotReceiveWithAnyArgs().AddPicture(default, default!, default);
	}

	[Fact]
	public async Task Handle_FileTooLarge_ThrowsInvalidPictureUpload()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new AddPictureCommandHandler(storage, repository);

		await Assert.ThrowsAsync<InvalidPictureUploadException>(() =>
			handler.Handle(ValidCommand(byteLength: 5 * 1024 * 1024 + 1), CancellationToken.None));

		await storage.DidNotReceiveWithAnyArgs().Save(default!, default!, default!, default);
		await repository.DidNotReceiveWithAnyArgs().AddPicture(default, default!, default);
	}

	[Fact]
	public async Task Handle_UnsupportedContentType_ThrowsInvalidPictureUpload()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new AddPictureCommandHandler(storage, repository);
		var command = ValidCommand() with { ContentType = "image/gif" };

		await Assert.ThrowsAsync<InvalidPictureUploadException>(() =>
			handler.Handle(command, CancellationToken.None));

		await storage.DidNotReceiveWithAnyArgs().Save(default!, default!, default!, default);
		await repository.DidNotReceiveWithAnyArgs().AddPicture(default, default!, default);
	}

	[Fact]
	public async Task Handle_UnsupportedExtension_ThrowsInvalidPictureUpload()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new AddPictureCommandHandler(storage, repository);
		var command = ValidCommand() with { Filename = "profile.gif" };

		await Assert.ThrowsAsync<InvalidPictureUploadException>(() =>
			handler.Handle(command, CancellationToken.None));

		await storage.DidNotReceiveWithAnyArgs().Save(default!, default!, default!, default);
		await repository.DidNotReceiveWithAnyArgs().AddPicture(default, default!, default);
	}

	[Fact]
	public async Task Handle_ValidUpload_SavesFileAddsPictureAndReturnsId()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new AddPictureCommandHandler(storage, repository);
		var command = ValidCommand();

		storage.Save(command.Stream, command.Filename, command.ContentType, CancellationToken.None)
			.Returns(Task.FromResult("/uploads/profile.png"));
		repository.AddPicture(command.UserId, "/uploads/profile.png", CancellationToken.None)
			.Returns(Task.FromResult(42));

		int result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(42, result);
		await storage.Received(1).Save(command.Stream, command.Filename, command.ContentType, CancellationToken.None);
		await repository.Received(1).AddPicture(command.UserId, "/uploads/profile.png", CancellationToken.None);
	}

	[Fact]
	public async Task Handle_RepositoryFailsAfterSave_DeletesSavedFileAndRethrows()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		var handler = new AddPictureCommandHandler(storage, repository);
		var command = ValidCommand();

		storage.Save(command.Stream, command.Filename, command.ContentType, CancellationToken.None)
			.Returns(Task.FromResult("/uploads/profile.png"));
		repository.AddPicture(command.UserId, "/uploads/profile.png", CancellationToken.None)
			.Returns<Task<int>>(_ => throw new PictureLimitExceededException());
		storage.Delete("/uploads/profile.png", CancellationToken.None)
			.Returns(Task.CompletedTask);

		await Assert.ThrowsAsync<PictureLimitExceededException>(() =>
			handler.Handle(command, CancellationToken.None));

		await storage.Received(1).Delete("/uploads/profile.png", CancellationToken.None);
	}
}
