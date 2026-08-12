using Application.Commands;
using Application.Interfaces;
using Domain.Entities.Authentication;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Matcha.Tests;

public class LoginCommandHandlerTests
{
	[Fact]
	public async Task Handle_ValidCredentials_StoresHashAndReturnsRawRefreshToken()
	{
		var accountRepository = Substitute.For<IUserRepository>();
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		var user = CreateUser();
		RefreshToken savedToken = null!;

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		accountRepository.GetByUsername("alice", CancellationToken.None)
			.Returns(Task.FromResult<User?>(user));
		passwordHasher.VerifyPassword("password", user.PasswordHash).Returns(true);
		tokenService.GenerateAccessToken(user).Returns("access-token");
		tokenService.GenerateRefreshToken().Returns("raw-refresh-token");
		tokenService.HashRefreshToken("raw-refresh-token").Returns("refresh-token-hash");
		refreshTokenRepository.Save(
			Arg.Do<RefreshToken>(value => savedToken = value),
			CancellationToken.None).Returns(Task.CompletedTask);
		var handler = new LoginCommandHandler(
			accountRepository,
			refreshTokenRepository,
			passwordHasher,
			tokenService,
			configuration);
		DateTime minimumExpiry = DateTime.UtcNow.AddDays(7);

		var result = await handler.Handle(
			new LoginCommand("alice", "password"),
			CancellationToken.None);

		Assert.Equal("access-token", result.AccessToken);
		Assert.Equal("raw-refresh-token", result.RefreshToken);
		Assert.Equal(user.Id, savedToken.UserId);
		Assert.Equal("refresh-token-hash", savedToken.TokenHash);
		Assert.InRange(savedToken.ExpiresAt, minimumExpiry, DateTime.UtcNow.AddDays(7));
	}

	[Fact]
	public async Task Handle_UnknownUser_ThrowsUserNotFoundException()
	{
		var accountRepository = Substitute.For<IUserRepository>();
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		accountRepository.GetByUsername("missing", CancellationToken.None)
			.Returns(Task.FromResult<User?>(null));
		var handler = new LoginCommandHandler(
			accountRepository,
			refreshTokenRepository,
			passwordHasher,
			tokenService,
			configuration);

		await Assert.ThrowsAsync<UserNotFoundException>(() => handler.Handle(
			new LoginCommand("missing", "password"),
			CancellationToken.None));

		await refreshTokenRepository.DidNotReceive()
			.Save(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_InvalidPassword_ThrowsInvalidPasswordException()
	{
		var accountRepository = Substitute.For<IUserRepository>();
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var passwordHasher = Substitute.For<IPasswordHasher>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		var user = CreateUser();

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		accountRepository.GetByUsername("alice", CancellationToken.None)
			.Returns(Task.FromResult<User?>(user));
		passwordHasher.VerifyPassword("wrong-password", user.PasswordHash).Returns(false);
		var handler = new LoginCommandHandler(
			accountRepository,
			refreshTokenRepository,
			passwordHasher,
			tokenService,
			configuration);

		await Assert.ThrowsAsync<InvalidPasswordException>(() => handler.Handle(
			new LoginCommand("alice", "wrong-password"),
			CancellationToken.None));

		await refreshTokenRepository.DidNotReceive()
			.Save(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
	}

	private static User CreateUser()
	{
		return new User
		{
			Id = 1,
			Username = "alice",
			PasswordHash = "password-hash",
			Email = "alice@example.com",
			FirstName = "Alice",
			LastName = "Doe"
		};
	}
}
