using Application.Commands;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using NSubstitute;

namespace Matcha.Tests;

public class LikeUserCommandHandlerTests
{
	[Fact]
	public async Task Handle_SelfLike_Throws()
	{
		var repository = Substitute.For<IUserRepository>();
		var handler = new LikeUserCommandHandler(repository);
		var command = new LikeUserCommand(1, 1);

		await Assert.ThrowsAsync<SelfLikeException>(() =>
			handler.Handle(command, CancellationToken.None));

		await repository.DidNotReceiveWithAnyArgs().LikeUser(default, default);
		await repository.DidNotReceiveWithAnyArgs().HasUserLiked(default, default);
	}

	[Fact]
	public async Task Handle_LikedUserNotFound_Throws()
	{
		var repository = Substitute.For<IUserRepository>();
		var handler = new LikeUserCommandHandler(repository);
		var command = new LikeUserCommand(1, 2);

		repository.GetById(command.LikedId)
			.Returns(Task.FromResult<User?>(null));

		await Assert.ThrowsAsync<UserNotFoundException>(() =>
			handler.Handle(command, CancellationToken.None));

		await repository.DidNotReceiveWithAnyArgs().LikeUser(default, default);
		await repository.DidNotReceiveWithAnyArgs().HasUserLiked(default, default);
	}

	[Fact]
	public async Task Handle_ReverseLikeDoesNotExist_ReturnsFalse()
	{
		var repository = Substitute.For<IUserRepository>();
		var handler = new LikeUserCommandHandler(repository);
		var command = new LikeUserCommand(1, 2);

		repository.GetById(command.LikedId)
			.Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));
		repository.HasUserLiked(command.LikedId, command.LikerId)
			.Returns(Task.FromResult(false));

		bool matched = await handler.Handle(command, CancellationToken.None);

		Assert.False(matched);
		await repository.Received(1).LikeUser(command.LikerId, command.LikedId);
		await repository.Received(1).HasUserLiked(command.LikedId, command.LikerId);
	}

	[Fact]
	public async Task Handle_ReverseLikeExists_ReturnsTrue()
	{
		var repository = Substitute.For<IUserRepository>();
		var handler = new LikeUserCommandHandler(repository);
		var command = new LikeUserCommand(1, 2);

		repository.GetById(command.LikedId)
			.Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));
		repository.HasUserLiked(command.LikedId, command.LikerId)
			.Returns(Task.FromResult(true));

		bool matched = await handler.Handle(command, CancellationToken.None);

		Assert.True(matched);
		await repository.Received(1).LikeUser(command.LikerId, command.LikedId);
		await repository.Received(1).HasUserLiked(command.LikedId, command.LikerId);
	}

	private static User CreateUser(int id)
	{
		return new User
		{
			Id = id,
			Username = $"user{id}",
			Password = "password",
			Email = $"user{id}@example.com",
			FirstName = "Test",
			LastName = "User"
		};
	}
}
