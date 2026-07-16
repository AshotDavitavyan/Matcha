using System.Security.Cryptography;
using Application.Dtos.AuthDtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler(IAuthRepository authRepository, ITokenService tokenService, IConfiguration configuration) :  IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
	public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		double tokenExpiryDays = double.Parse(configuration["Jwt:RefreshTokenExpiryDays"]!);
		
		User? user = await authRepository.GetByRefreshToken(request.RefreshToken, cancellationToken);
		if (user is null)
		{
			throw new InvalidRefreshTokenException();
		}

		if (user.RefreshTokenExpiry < DateTime.UtcNow)
		{
			throw new InvalidRefreshTokenException();
		}

		string token = tokenService.GenerateToken(user);
		string refreshToken =  Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		await authRepository.SaveRefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(tokenExpiryDays), cancellationToken);
		return new AuthResponseDto(token, refreshToken);
	}
}