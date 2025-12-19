using Microsoft.AspNetCore.Mvc;
using Readlog.Application.Shared;

namespace Readlog.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return ToProblemResult(result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ToProblemResult(result.Error);
    }

    public static IActionResult ToCreatedResult<T>(this Result<T> result, string routeName, Func<T, object> routeValues)
    {
        if (result.IsSuccess)
            return new CreatedAtRouteResult(routeName, routeValues(result.Value), result.Value);

        return ToProblemResult(result.Error);
    }

    private static ObjectResult ToProblemResult(Error error)
    {
        var (statusCode, title) = error.Code switch
        {
            var code when code.Contains("NotFound") => (StatusCodes.Status404NotFound, "Not Found"),
            var code when code.Contains("Validation") => (StatusCodes.Status400BadRequest, "Validation Error"),
            var code when code.Contains("Conflict") => (StatusCodes.Status409Conflict, "Conflict"),
            var code when code.Contains("Unauthorized") => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status400BadRequest, "Bad Request")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = error.Message
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
