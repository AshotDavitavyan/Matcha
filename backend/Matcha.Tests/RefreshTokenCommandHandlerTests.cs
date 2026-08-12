using Application.Commands;
using Application.Interfaces;
using Domain.Entities.Authentication;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Matcha.Tests;

public class RefreshTokenCommandHandlerTests
{
	[Fact]
	public async Task Handle_ValidRefresh_RotatesRefreshTokenAndReturnsNewTokens()
	{
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var accountRepository = Substitute.For<IUserRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		var user = CreateUser(1);
		var storedToken = new RefreshToken
		{
			UserId = user.Id,
			TokenHash = "presented-token-hash",
			ExpiresAt = DateTime.UtcNow.AddDays(1)
		};
		RefreshToken savedToken = null!;

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		tokenService.HashRefreshToken("presented-token").Returns("presented-token-hash");
		refreshTokenRepository.GetByTokenHash("presented-token-hash", CancellationToken.None)
			.Returns(Task.FromResult<RefreshToken?>(storedToken));
		accountRepository.GetById(user.Id, CancellationToken.None)
			.Returns(Task.FromResult<User?>(user));
		tokenService.GenerateAccessToken(user).Returns("new-access-token");
		tokenService.GenerateRefreshToken().Returns("new-refresh-token");
		tokenService.HashRefreshToken("new-refresh-token").Returns("new-refresh-token-hash");
		refreshTokenRepository.Save(
			Arg.Do<RefreshToken>(value => savedToken = value),
			CancellationToken.None).Returns(Task.CompletedTask);
		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			tokenService,
			configuration,
			accountRepository);
		DateTime minimumExpiry = DateTime.UtcNow.AddDays(7);

		var result = await handler.Handle(
			new RefreshTokenCommand("presented-token"),
			CancellationToken.None);

		Assert.Equal("new-access-token", result.AccessToken);
		Assert.Equal("new-refresh-token", result.RefreshToken);
		Assert.Equal(user.Id, savedToken.UserId);
		Assert.Equal("new-refresh-token-hash", savedToken.TokenHash);
		Assert.InRange(savedToken.ExpiresAt, minimumExpiry, DateTime.UtcNow.AddDays(7));
		await refreshTokenRepository.Received(1)
			.GetByTokenHash("presented-token-hash", CancellationToken.None);
		await accountRepository.Received(1).GetById(user.Id, CancellationToken.None);
		await refreshTokenRepository.Received(1)
			.Save(Arg.Any<RefreshToken>(), CancellationToken.None);
	}

	[Fact]
	public async Task Handle_UnknownRefreshToken_ThrowsInvalidRefreshTokenException()
	{
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var accountRepository = Substitute.For<IUserRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		tokenService.HashRefreshToken("unknown-token").Returns("unknown-token-hash");
		refreshTokenRepository.GetByTokenHash("unknown-token-hash", CancellationToken.None)
			.Returns(Task.FromResult<RefreshToken?>(null));
		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			tokenService,
			configuration,
			accountRepository);

		await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => handler.Handle(
			new RefreshTokenCommand("unknown-token"),
			CancellationToken.None));

		await accountRepository.DidNotReceive()
			.GetById(Arg.Any<int>(), Arg.Any<CancellationToken>());
		await refreshTokenRepository.DidNotReceive()
			.Save(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ExpiredRefreshToken_ThrowsInvalidRefreshTokenException()
	{
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var accountRepository = Substitute.For<IUserRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		var storedToken = new RefreshToken
		{
			UserId = 1,
			TokenHash = "expired-token-hash",
			ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
		};

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		tokenService.HashRefreshToken("expired-token").Returns("expired-token-hash");
		refreshTokenRepository.GetByTokenHash("expired-token-hash", CancellationToken.None)
			.Returns(Task.FromResult<RefreshToken?>(storedToken));
		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			tokenService,
			configuration,
			accountRepository);

		await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => handler.Handle(
			new RefreshTokenCommand("expired-token"),
			CancellationToken.None));

		await accountRepository.DidNotReceive()
			.GetById(Arg.Any<int>(), Arg.Any<CancellationToken>());
		await refreshTokenRepository.DidNotReceive()
			.Save(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_MissingUser_ThrowsInvalidRefreshTokenException()
	{
		var refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
		var accountRepository = Substitute.For<IUserRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		var storedToken = new RefreshToken
		{
			UserId = 1,
			TokenHash = "valid-token-hash",
			ExpiresAt = DateTime.UtcNow.AddDays(1)
		};

		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		tokenService.HashRefreshToken("valid-token").Returns("valid-token-hash");
		refreshTokenRepository.GetByTokenHash("valid-token-hash", CancellationToken.None)
			.Returns(Task.FromResult<RefreshToken?>(storedToken));
		accountRepository.GetById(storedToken.UserId, CancellationToken.None)
			.Returns(Task.FromResult<User?>(null));
		var handler = new RefreshTokenCommandHandler(
			refreshTokenRepository,
			tokenService,
			configuration,
			accountRepository);

		await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => handler.Handle(
			new RefreshTokenCommand("valid-token"),
			CancellationToken.None));

		await refreshTokenRepository.DidNotReceive()
			.Save(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
	}

	private static User CreateUser(int id)
	{
		return new User
		{
			Id = id,
			Username = "testuser",
			PasswordHash = "password-hash",
			Email = "test@test.com",
			FirstName = "Test",
			LastName = "User"
		};
	}
}
