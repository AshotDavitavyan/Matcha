using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace matcha_app.Exceptions;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            InvalidRefreshTokenException => StatusCodes.Status401Unauthorized,
            UserNotFoundException => StatusCodes.Status404NotFound,
            PictureNotFoundException => StatusCodes.Status404NotFound,
            InvalidPasswordException => StatusCodes.Status422UnprocessableEntity,
            SamePasswordException => StatusCodes.Status422UnprocessableEntity,
            PictureLimitExceededException => StatusCodes.Status422UnprocessableEntity,
            InvalidPictureUploadException => StatusCodes.Status422UnprocessableEntity,
            SelfLikeException => StatusCodes.Status422UnprocessableEntity,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = exception.Message
            }
        });

        return true;
    }
}
