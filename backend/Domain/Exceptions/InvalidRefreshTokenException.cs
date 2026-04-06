namespace Domain.Exceptions;

public class InvalidRefreshTokenException() : DomainException("Invalid or expired refresh token.");
