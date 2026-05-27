namespace Domain.Exceptions;

public class SelfLikeException() : DomainException("You cannot like your own profile.");