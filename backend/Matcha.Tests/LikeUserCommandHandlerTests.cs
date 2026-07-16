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
		var repository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new LikeUserCommandHandler(repository, accountRepository);
		var command = new LikeUserCommand(1, 1);

		await Assert.ThrowsAsync<SelfLikeException>(() =>
			handler.Handle(command, CancellationToken.None));

		await repository.DidNotReceiveWithAnyArgs().LikeUser(default, default, default);
		await repository.DidNotReceiveWithAnyArgs().HasUserLiked(default, default, default);
	}

	[Fact]
	public async Task Handle_LikedUserNotFound_Throws()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new LikeUserCommandHandler(likeRepository, accountRepository);
		var command = new LikeUserCommand(1, 2);

		accountRepository.GetById(command.LikedId, CancellationToken.None)
			.Returns(Task.FromResult<User?>(null));

		await Assert.ThrowsAsync<UserNotFoundException>(() =>
			handler.Handle(command, CancellationToken.None));

		await likeRepository.DidNotReceiveWithAnyArgs().LikeUser(default, default, default);
		await likeRepository.DidNotReceiveWithAnyArgs().HasUserLiked(default, default, default);
	}

	[Fact]
	public async Task Handle_ReverseLikeDoesNotExist_ReturnsFalse()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new LikeUserCommandHandler(likeRepository, accountRepository);
		var command = new LikeUserCommand(1, 2);

		accountRepository.GetById(command.LikedId, CancellationToken.None)
			.Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));
		likeRepository.HasUserLiked(command.LikedId, command.LikerId, CancellationToken.None)
			.Returns(Task.FromResult(false));

		bool matched = await handler.Handle(command, CancellationToken.None);

		Assert.False(matched);
		await likeRepository.Received(1).LikeUser(command.LikerId, command.LikedId, CancellationToken.None);
		await likeRepository.Received(1).HasUserLiked(command.LikedId, command.LikerId, CancellationToken.None);
	}

	[Fact]
	public async Task Handle_ReverseLikeExists_ReturnsTrue()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new LikeUserCommandHandler(likeRepository, accountRepository);
		var command = new LikeUserCommand(1, 2);

		accountRepository.GetById(command.LikedId, CancellationToken.None)
			.Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));
		likeRepository.HasUserLiked(command.LikedId, command.LikerId, CancellationToken.None)
			.Returns(Task.FromResult(true));

		bool matched = await handler.Handle(command, CancellationToken.None);

		Assert.True(matched);
		await likeRepository.Received(1).LikeUser(command.LikerId, command.LikedId, CancellationToken.None);
		await likeRepository.Received(1).HasUserLiked(command.LikedId, command.LikerId, CancellationToken.None);
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
