namespace Domain.Exceptions;

public class UserNotFoundException(string identifier) : DomainException($"User '{identifier}' was not found.");
