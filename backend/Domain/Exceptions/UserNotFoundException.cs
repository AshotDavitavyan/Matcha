namespace Domain.Exceptions;

public class UserNotFoundException(int id) : DomainException($"User with id {id} was not found.");
