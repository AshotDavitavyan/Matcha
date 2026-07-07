using Application.Commands;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using NSubstitute;

namespace Matcha.Tests;

public class UnlikeUserCommandHandlerTests
{
	[Fact]
	public async Task Handle_SelfUnlike_Throws()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new UnlikeUserCommandHandler(likeRepository, accountRepository);
		var command = new UnlikeUserCommand(1, 1);

		await Assert.ThrowsAsync<SelfLikeException>(() =>
			handler.Handle(command, CancellationToken.None));

		await likeRepository.DidNotReceiveWithAnyArgs().UnlikeUser(default, default);
	}

	[Fact]
	public async Task Handle_LikedUserNotFound_Throws()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new UnlikeUserCommandHandler(likeRepository, accountRepository);
		var command = new UnlikeUserCommand(1, 2);

		accountRepository.GetById(command.LikedId)
			.Returns(Task.FromResult<User?>(null));

		await Assert.ThrowsAsync<UserNotFoundException>(() =>
			handler.Handle(command, CancellationToken.None));

		await likeRepository.DidNotReceiveWithAnyArgs().UnlikeUser(default, default);
	}

	[Fact]
	public async Task Handle_ExistingLikedUser_CallsRepository()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var handler = new UnlikeUserCommandHandler(likeRepository, accountRepository);
		var command = new UnlikeUserCommand(1, 2);

		accountRepository.GetById(command.LikedId)
			.Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));

		await handler.Handle(command, CancellationToken.None);

		await likeRepository.Received(1).UnlikeUser(command.LikerId, command.LikedId);
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
