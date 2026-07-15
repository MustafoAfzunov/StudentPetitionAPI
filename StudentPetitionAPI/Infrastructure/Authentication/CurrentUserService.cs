using System.Security.Claims;
using StudentPetitionAPI.Infrastructure.Authentication;
using StudentPetitionAPI.Application.Interfaces;

namespace StudentPetitionAPI.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("email");

    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role);

    public bool IsStudent => User?.IsInRole(Roles.Student) == true;

    public bool IsReviewer => User?.IsInRole(Roles.Reviewer) == true;
}
