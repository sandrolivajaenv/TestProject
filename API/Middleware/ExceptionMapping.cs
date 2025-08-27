using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;


public static class ExceptionMapping
{
    public static void MapProblemDetails(Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options)
    {
        options.Map<NotFound>(ex => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = ex.Message
        });

        options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized);
        options.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest);
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }
}