using Application.Dtos.AuthDtos;
using Application.Interfaces;
using Domain.Entities.Authentication;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService, IConfiguration configuration, IUserRepository repository) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
	public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		double tokenExpiryDays = double.Parse(configuration["Jwt:RefreshTokenExpiryDays"]!);
		string tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
		RefreshToken? token = await refreshTokenRepository.GetByTokenHash(tokenHash, cancellationToken);
		if (token is null || token.ExpiresAt <= DateTime.UtcNow)
		{
			throw new InvalidRefreshTokenException();
		}

		User? user = await repository.GetById(token.UserId, cancellationToken);

		if (user is null)
		{
			throw new InvalidRefreshTokenException();
		}

		string newAccessToken = tokenService.GenerateAccessToken(user);
		string newRefreshToken = tokenService.GenerateRefreshToken();
		string newHashedRefreshToken = tokenService.HashRefreshToken(newRefreshToken);
		await refreshTokenRepository.Save(new RefreshToken()
		{
			UserId = user.Id,
			TokenHash = newHashedRefreshToken,
			ExpiresAt = DateTime.UtcNow.AddDays(tokenExpiryDays)
		}, cancellationToken);
		return new AuthResponseDto(newAccessToken, newRefreshToken);
	}
}
