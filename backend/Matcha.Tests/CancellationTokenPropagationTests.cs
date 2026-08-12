using Application.Commands;
using Application.Dtos.UserDtos;
using Application.Interfaces;
using Application.Queries;
using Domain.Entities.Authentication;
using Domain.Entities.Users;
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
		var repository = Substitute.For<IUserRepository>();
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
		var accountRepository = Substitute.For<IUserRepository>();
		var authRepository = Substitute.For<IRefreshTokenRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var user = CreateUser(7);
		user.PasswordHash = "hashed-password";
		var handler = new LoginCommandHandler(accountRepository, authRepository, passwordHasher, tokenService, configuration);

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		accountRepository.GetByUsername("alice", token).Returns(Task.FromResult<User?>(user));
		passwordHasher.VerifyPassword("password", user.PasswordHash).Returns(true);
		tokenService.GenerateAccessToken(user).Returns("access-token");
		tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
		tokenService.HashRefreshToken("raw-refresh-token").Returns("refresh-token-hash");
		authRepository.Save(Arg.Any<RefreshToken>(), token)
			.Returns(Task.CompletedTask);

		await handler.Handle(new LoginCommand("alice", "password"), token);

		await accountRepository.Received(1).GetByUsername("alice", token);
		await authRepository.Received(1).Save(
			Arg.Is<RefreshToken>(refreshToken =>
				refreshToken.UserId == user.Id &&
				refreshToken.TokenHash == "refresh-token-hash"),
			token);
	}

	[Fact]
	public async Task RefreshTokenCommand_ForwardsTokenToRepositories()
	{
		var authRepository = Substitute.For<IRefreshTokenRepository>();
		var accountRepository = Substitute.For<IUserRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var user = CreateUser(7);
		var storedToken = new RefreshToken
		{
			UserId = user.Id,
			TokenHash = "refresh-token-hash",
			ExpiresAt = DateTime.UtcNow.AddDays(1)
		};
		var handler = new RefreshTokenCommandHandler(
			authRepository,
			tokenService,
			configuration,
			accountRepository);

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		tokenService.HashRefreshToken("refresh-token").Returns("refresh-token-hash");
		authRepository.GetByTokenHash("refresh-token-hash", token)
			.Returns(Task.FromResult<RefreshToken?>(storedToken));
		accountRepository.GetById(user.Id, token).Returns(Task.FromResult<User?>(user));
		tokenService.GenerateAccessToken(user).Returns("access-token");
		tokenService.GenerateRefreshToken().Returns("new-refresh-token");
		tokenService.HashRefreshToken("new-refresh-token").Returns("new-refresh-token-hash");
		authRepository.Save(Arg.Any<RefreshToken>(), token)
			.Returns(Task.CompletedTask);

		await handler.Handle(new RefreshTokenCommand("refresh-token"), token);

		await authRepository.Received(1).GetByTokenHash("refresh-token-hash", token);
		await accountRepository.Received(1).GetById(user.Id, token);
		await authRepository.Received(1).Save(
			Arg.Is<RefreshToken>(refreshToken =>
				refreshToken.UserId == user.Id &&
				refreshToken.TokenHash == "new-refresh-token-hash"),
			token);
	}

	[Fact]
	public async Task LogoutCommand_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IRefreshTokenRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new LogoutCommandHandler(repository);

		repository.DeleteByUserId(7, token).Returns(Task.CompletedTask);

		await handler.Handle(new LogoutCommand(7), token);

		await repository.Received(1).DeleteByUserId(7, token);
	}

	[Fact]
	public async Task UpdatePasswordCommand_ForwardsTokenToRepositoryCalls()
	{
		var repository = Substitute.For<IUserRepository>();
		var hasher = Substitute.For<IPasswordHasher>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var user = CreateUser(7);
		user.PasswordHash = "old-hash";
		var handler = new UpdatePasswordCommandHandler(repository, hasher);
		var dto = new UpdatePasswordDto { CurrentPassword = "old-password", NewPassword = "new-password" };

		hasher.HashPassword(dto.NewPassword).Returns("new-hash");
		repository.GetById(7, token).Returns(Task.FromResult<User?>(user));
		hasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash).Returns(true);
		hasher.VerifyPassword(dto.NewPassword, user.PasswordHash).Returns(false);
		repository.UpdatePassword(7, "new-hash", token).Returns(Task.CompletedTask);

		await handler.Handle(new UpdatePasswordCommand(7, dto), token);

		await repository.Received(1).GetById(7, token);
		await repository.Received(1).UpdatePassword(7, "new-hash", token);
	}

	[Fact]
	public async Task UpdateUserCommand_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new UpdateUserCommandHandler(repository);
		var dto = new UpdateUserDto
		{
			FirstName = "Alice",
			LastName = "Doe",
			Email = "alice@example.com",
			Biography = "Bio",
			Gender = Gender.Female,
			SexualPreference = SexualPreference.Both,
			Tags = new List<string> { "music", "coffee" }
		};

		repository.UpdateUser(Arg.Any<User>(), token).Returns(Task.CompletedTask);

		await handler.Handle(new UpdateUserCommand(7, dto), token);

		await repository.Received(1).UpdateUser(Arg.Any<User>(), token);
	}

	[Fact]
	public async Task GetAllUsersQuery_ForwardsTokenToRepository()
	{
		var repository = Substitute.For<IUserRepository>();
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
		var repository = Substitute.For<IUserRepository>();
		var pictureRepository = Substitute.For<IUserPictureRepository>();
		using var cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		var handler = new GetUserByIdQueryHandler(repository, pictureRepository);

		repository.GetById(7, token).Returns(Task.FromResult<User?>(CreateUser(7)));
		pictureRepository.GetPicturesByUserId(7, token)
			.Returns(Task.FromResult(new List<Picture>()));

		await handler.Handle(new GetUserByIdQuery(7), token);

		await repository.Received(1).GetById(7, token);
		await pictureRepository.Received(1).GetPicturesByUserId(7, token);
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
		var accountRepository = Substitute.For<IUserRepository>();
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
		var accountRepository = Substitute.For<IUserRepository>();
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
			PasswordHash = "password",
			Email = $"user{id}@example.com",
			FirstName = "Test",
			LastName = "User"
		};
	}

}
