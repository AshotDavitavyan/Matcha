using System.Security.Cryptography;
using Application.Dtos.AuthDtos;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;


namespace Application.Commands;

public record LoginCommand(string Username, string Password) : IRequest<AuthResponseDto>;

public class LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IConfiguration configuration) : IRequestHandler<LoginCommand, AuthResponseDto>
{
	public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		double tokenExpiryDays = double.Parse(configuration["Jwt:RefreshTokenExpiryDays"]!);
		var user = await userRepository.GetByUsername(request.Username)
			?? throw new UserNotFoundException(request.Username);

		if (!passwordHasher.VerifyPassword(request.Password, user.Password))
			throw new InvalidPasswordException();

		string token = tokenService.GenerateToken(user);
		string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		await userRepository.SaveRefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(tokenExpiryDays));
		return new AuthResponseDto(token, refreshToken);
	}
}