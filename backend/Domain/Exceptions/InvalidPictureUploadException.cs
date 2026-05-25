namespace Domain.Exceptions;

public class InvalidPictureUploadException(string message) : DomainException(message);