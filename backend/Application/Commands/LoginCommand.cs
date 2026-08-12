using Application.Dtos.AuthDtos;
using Application.Interfaces;
using Domain.Entities.Authentication;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Commands;

public record LoginCommand(string Username, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler(IUserRepository userRepository, IRefreshTokenRepository authRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IConfiguration configuration) : IRequestHandler<LoginCommand, AuthResponseDto>
{
	public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		double tokenExpiryDays = double.Parse(configuration["Jwt:RefreshTokenExpiryDays"]!);
		var user = await userRepository.GetByUsername(request.Username, cancellationToken)
			?? throw new UserNotFoundException(request.Username);

		if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
			throw new InvalidPasswordException();

		string token = tokenService.GenerateAccessToken(user);
		string refreshToken = tokenService.GenerateRefreshToken();
		string hashedRefreshToken = tokenService.HashRefreshToken(refreshToken);

		await authRepository.Save(new RefreshToken()
		{
			UserId = user.Id,
			TokenHash = hashedRefreshToken,
			ExpiresAt = DateTime.UtcNow.AddDays(tokenExpiryDays)
		}, cancellationToken);
		return new AuthResponseDto(token, refreshToken);
	}
}
