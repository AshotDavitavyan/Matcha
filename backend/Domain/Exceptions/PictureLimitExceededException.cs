namespace Domain.Exceptions;

public class PictureLimitExceededException() : DomainException("A user may have at most 5 pictures");