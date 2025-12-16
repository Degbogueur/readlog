using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Readlog.Domain.Exceptions;

namespace Readlog.Api.Handlers;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            FluentValidation.ValidationException validationException => CreateValidationProblemDetails(validationException),
            BaseException baseException => CreateDomainProblemDetails(baseException),
            _ => CreateServerErrorProblemDetails()
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateValidationProblemDetails(FluentValidation.ValidationException exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Extensions =
            {
                ["errors"] = exception.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    )
            }
        };
    }

    private static ProblemDetails CreateDomainProblemDetails(BaseException exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Domain Error",
            Detail = exception.Message
        };
    }

    private static ProblemDetails CreateServerErrorProblemDetails()
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred."
        };
    }
}
