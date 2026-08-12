using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Entities.Users;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Infrastructure.Security;

public class TokenService(IConfiguration configuration) : ITokenService
{
	public string GenerateAccessToken(User user)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));

		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim("username", user.Username),
		};

		var token = new JwtSecurityToken(
			issuer: configuration["Jwt:Issuer"],
			audience: configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(
				double.Parse(configuration["Jwt:ExpiresInMinutes"]!)),
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		return refreshToken;
	}

	public string HashRefreshToken(string refreshToken)
	{
		byte[] tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
		byte[] hashBytes = SHA256.HashData(tokenBytes);

		return Convert.ToHexString(hashBytes);
	}
}
