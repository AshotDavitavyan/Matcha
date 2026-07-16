using Application.Commands;
using Domain.Repositories;
using NSubstitute;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Matcha.Tests;

public class RefreshTokenCommandHandlerTests
{
	[Fact]
	public async Task Handle_ValidRefresh_Ok()
	{
		var userRepository = Substitute.For<IAuthRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();

		var user = new User
		{
			Id = 1,
			Username = "testuser",
			Email = "test@test.com",
			RefreshToken = "valid-token",
			RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
		};

		userRepository.GetByRefreshToken("valid-token", CancellationToken.None).Returns(user);
		tokenService.GenerateToken(Arg.Any<User>()).Returns("new-access-token");
		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		
		var handler = new RefreshTokenCommandHandler(userRepository, tokenService, configuration);
		var result = await handler.Handle(new RefreshTokenCommand("valid-token"), CancellationToken.None);
		
		Assert.NotNull(result);
		Assert.Equal("new-access-token", result.AccessToken);
		Assert.NotEmpty(result.RefreshToken);
	}
	
	[Fact]
	public async Task Handle_InvalidRefresh_Invalid()
	{
		var userRepository = Substitute.For<IAuthRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();
		
		userRepository.GetByRefreshToken("invalid-token", CancellationToken.None).Returns((User?)null);
		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		var handler = new RefreshTokenCommandHandler(userRepository, tokenService, configuration);
		await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => handler.Handle(new RefreshTokenCommand("invalid-token"), CancellationToken.None));
	}

	[Fact]
	public async Task Handle_ExpiredRefresh_Invalid()
	{
		var userRepository = Substitute.For<IAuthRepository>();
		var tokenService = Substitute.For<ITokenService>();
		var configuration = Substitute.For<IConfiguration>();

		User user = new User
		{
			Id = 1,
			Username = "testuser",
			Email = "test@test.com",
			RefreshToken = "expired-token",
			RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1)
		};
		
		userRepository.GetByRefreshToken("expired-token", CancellationToken.None).Returns(user);
		configuration["Jwt:RefreshTokenExpiryDays"].Returns("7");
		
		var handler = new RefreshTokenCommandHandler(userRepository, tokenService, configuration);
		await Assert.ThrowsAsync<InvalidRefreshTokenException>(() => handler.Handle(new RefreshTokenCommand("expired-token"), CancellationToken.None));
	}
}
