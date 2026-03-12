namespace Domain.Exceptions;

public class SamePasswordException() : DomainException("The new password is the same as the current one.");
