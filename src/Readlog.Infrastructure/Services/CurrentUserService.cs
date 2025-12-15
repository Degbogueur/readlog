using Microsoft.AspNetCore.Http;
using Readlog.Application.Abstractions;
using System.Security.Claims;

namespace Readlog.Infrastructure.Services;

public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId => 
        Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)
        ? userId
        : Guid.Empty;

    public bool IsAuthenticated => 
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
