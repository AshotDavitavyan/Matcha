using Application.Commands;
using Application.Dtos.UserDtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Matcha.Tests;

public class CancellationTokenPropagationTests
{
	[Fact]
	public async Task CreateUserCommand_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserAccountRepository>();
		var hasher = Substitute.For<IPasswordHasher>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new CreateUserCommandHandler(repository, hasher);
		var command = new CreateUserCommand("alice", "Alice", "Doe", "alice@example.com", "password");

		hasher.HashPassword(command.Password).Returns("hashed-password");
		repository.Create(Arg.Any<User>(), token).Returns(Task.FromResult(42));

		int id = await handler.Handle(command, token);

		Assert.Equal(42, id);
		await repository.Received(1).Create(Arg.Any<User>(), token);
	}

	[Fact]
	public async Task LoginCommand_ForwardsTokenToRepositories()
	{
		var accountRepository = Substitute.For<IUserAccountRepository>();
		var authRepository = Substitute.For<IAuthRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var user = CreateUser(7);
		user.Password = "hashed-password";
		var handler = new LoginCommandHandler(accountRepository, authRepository, passwordHasher, tokenService, configuration);

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		accountRepository.GetByUsername("alice", token).Returns(Task.FromResult<User?>(user));
		passwordHasher.VerifyPassword("password", user.Password).Returns(true);
		tokenService.GenerateToken(user).Returns("access-token");
		authRepository.SaveRefreshToken(user.Id, Arg.Any<string>(), Arg.Any<DateTime>(), token)
			.Returns(Task.CompletedTask);

		await handler.Handle(new LoginCommand("alice", "password"), token);

		await accountRepository.Received(1).GetByUsername("alice", token);
		await authRepository.Received(1).SaveRefreshToken(user.Id, Arg.Any<string>(), Arg.Any<DateTime>(), token);
	}

	[Fact]
	public async Task RefreshTokenCommand_ForwardsTokenToRepositories()
	{
		var authRepository = Substitute.For<IAuthRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var user = CreateUser(7);
		user.RefreshToken = "refresh-token";
		user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(1);
		var handler = new RefreshTokenCommandHandler(authRepository, tokenService, configuration);

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		authRepository.GetByRefreshToken("refresh-token", token).Returns(Task.FromResult<User?>(user));
		tokenService.GenerateToken(user).Returns("access-token");
		authRepository.SaveRefreshToken(user.Id, Arg.Any<string>(), Arg.Any<DateTime>(), token)
			.Returns(Task.CompletedTask);

		await handler.Handle(new RefreshTokenCommand("refresh-token"), token);

		await authRepository.Received(1).GetByRefreshToken("refresh-token", token);
		await authRepository.Received(1).SaveRefreshToken(user.Id, Arg.Any<string>(), Arg.Any<DateTime>(), token);
	}

	[Fact]
	public async Task LogoutCommand_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IAuthRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new LogoutCommandHandler(repository);

		repository.ClearRefreshToken(7, token).Returns(Task.CompletedTask);

		await handler.Handle(new LogoutCommand(7), token);

		await repository.Received(1).ClearRefreshToken(7, token);
	}

	[Fact]
	public async Task UpdatePasswordCommand_ForwardsTokenToRepositoryCalls()
	{
		var repository = Substitute.For<IUserAccountRepository>();
		var hasher = Substitute.For<IPasswordHasher>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var user = CreateUser(7);
		user.Password = "old-hash";
		var handler = new UpdatePasswordCommandHandler(repository, hasher);
		var dto = new UpdatePasswordDto { CurrentPassword = "old-password", NewPassword = "new-password" };

		hasher.HashPassword(dto.NewPassword).Returns("new-hash");
		repository.GetById(7, token).Returns(Task.FromResult<User?>(user));
		hasher.VerifyPassword(dto.CurrentPassword, user.Password).Returns(true);
		hasher.VerifyPassword(dto.NewPassword, user.Password).Returns(false);
		repository.UpdatePassword(7, "new-hash", token).Returns(Task.CompletedTask);

		await handler.Handle(new UpdatePasswordCommand(7, dto), token);

		await repository.Received(1).GetById(7, token);
		await repository.Received(1).UpdatePassword(7, "new-hash", token);
	}

	[Fact]
	public async Task UpdateProfileCommand_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserProfileRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new UpdateProfileCommandHandler(repository);
		var dto = new UpdateUserProfileDto
		{
			FirstName = "Alice",
			LastName = "Doe",
			Email = "alice@example.com",
			Biography = "Bio",
			Gender = Gender.Female,
			SexualPreference = SexualPreference.Both,
			Tags = new List<string> { "music", "coffee" }
		};

		repository.UpdateProfile(Arg.Any<UserProfile>(), token).Returns(Task.CompletedTask);

		await handler.Handle(new UpdateProfileCommand(7, dto), token);

		await repository.Received(1).UpdateProfile(Arg.Any<UserProfile>(), token);
	}

	[Fact]
	public async Task GetAllUsersQuery_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserAccountRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new GetAllUsersCommandHandler(repository);

		repository.GetAll(token).Returns(Task.FromResult<IList<User>>(new List<User> { CreateUser(7) }));

		await handler.Handle(new GetAllUsersQuery(), token);

		await repository.Received(1).GetAll(token);
	}

	[Fact]
	public async Task GetUserByIdQuery_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserAccountRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new GetUserByIdQueryHandler(repository);

		repository.GetById(7, token).Returns(Task.FromResult<User?>(CreateUser(7)));

		await handler.Handle(new GetUserByIdQuery(7), token);

		await repository.Received(1).GetById(7, token);
	}

	[Fact]
	public async Task GetUserProfileQuery_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserProfileRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new GetUserProfileQueryHandler(repository);

		repository.GetUserProfile(7, token).Returns(Task.FromResult<UserProfile?>(CreateProfile(7)));

		await handler.Handle(new GetUserProfileQuery(7), token);

		await repository.Received(1).GetUserProfile(7, token);
	}

	[Fact]
	public async Task AddPictureCommand_ForwardsTokenToStorageAndRepository()
	{
		var storage = Substitute.For<IPictureStorage>();
		var repository = Substitute.For<IUserPictureRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new AddPictureCommandHandler(storage, repository);
		var stream = new MemoryStream([1, 2, 3]);
		var command = new AddPictureCommand(7, stream, "profile.png", "image/png", 3);

		storage.Save(stream, command.Filename, command.ContentType, token)
			.Returns(Task.FromResult("/uploads/profile.png"));
		repository.AddPicture(command.UserId, "/uploads/profile.png", token).Returns(Task.FromResult(42));

		await handler.Handle(command, token);

		await storage.Received(1).Save(stream, command.Filename, command.ContentType, token);
		await repository.Received(1).AddPicture(command.UserId, "/uploads/profile.png", token);
	}

	[Fact]
	public async Task DeletePictureCommand_ForwardsTokenToRepositoryAndStorage()
	{
		var repository = Substitute.For<IUserPictureRepository>();
		var storage = Substitute.For<IPictureStorage>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new DeletePictureCommandHandler(repository, storage);
		var command = new DeletePictureCommand(7, 42);

		repository.RemovePicture(command.UserId, command.PictureId, token)
			.Returns(Task.FromResult("/uploads/profile.png"));
		storage.Delete("/uploads/profile.png", token).Returns(Task.CompletedTask);

		await handler.Handle(command, token);

		await repository.Received(1).RemovePicture(command.UserId, command.PictureId, token);
		await storage.Received(1).Delete("/uploads/profile.png", token);
	}

	[Fact]
	public async Task SetProfilePictureCommand_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserPictureRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new SetProfilePictureCommandHandler(repository);
		var command = new SetProfilePictureCommand(7, 42);

		repository.SetProfilePicture(command.UserId, command.PictureId, token).Returns(Task.CompletedTask);

		await handler.Handle(command, token);

		await repository.Received(1).SetProfilePicture(command.UserId, command.PictureId, token);
	}

	[Fact]
	public async Task LikeUserCommand_ForwardsTokenToRepositories()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new LikeUserCommandHandler(likeRepository, accountRepository);
		var command = new LikeUserCommand(7, 8);

		accountRepository.GetById(command.LikedId, token).Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));
		likeRepository.LikeUser(command.LikerId, command.LikedId, token).Returns(Task.CompletedTask);
		likeRepository.HasUserLiked(command.LikedId, command.LikerId, token).Returns(Task.FromResult(false));

		await handler.Handle(command, token);

		await accountRepository.Received(1).GetById(command.LikedId, token);
		await likeRepository.Received(1).LikeUser(command.LikerId, command.LikedId, token);
		await likeRepository.Received(1).HasUserLiked(command.LikedId, command.LikerId, token);
	}

	[Fact]
	public async Task UnlikeUserCommand_ForwardsTokenToRepositories()
	{
		var likeRepository = Substitute.For<ILikeRepository>();
		var accountRepository = Substitute.For<IUserAccountRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new UnlikeUserCommandHandler(likeRepository, accountRepository);
		var command = new UnlikeUserCommand(7, 8);

		accountRepository.GetById(command.LikedId, token).Returns(Task.FromResult<User?>(CreateUser(command.LikedId)));
		likeRepository.UnlikeUser(command.LikerId, command.LikedId, token).Returns(Task.CompletedTask);

		await handler.Handle(command, token);

		await accountRepository.Received(1).GetById(command.LikedId, token);
		await likeRepository.Received(1).UnlikeUser(command.LikerId, command.LikedId, token);
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

	private static UserProfile CreateProfile(int id)
	{
		return new UserProfile
		{
			Id = id,
			Username = $"user{id}",
			FirstName = "Test",
			LastName = "User",
			Email = $"user{id}@example.com",
			Biography = "Bio",
			Gender = Gender.Female,
			SexualPreference = SexualPreference.Both
		};
	}
}
