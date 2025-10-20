using System.Security.Claims;
using HRS.Shared.Core.Interfaces;
using HRS.Shared.Core.Dtos;

namespace HRS.API.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ??
                         _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    public string? GetEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value ??
               _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public async Task<UserResponseDto> GetUserAsync()
    {
        var userId = GetUserId();
        var email = GetEmail();
        var firstName = _httpContextAccessor.HttpContext?.User?.FindFirst("given_name")?.Value ?? "Unknown";
        var lastName = _httpContextAccessor.HttpContext?.User?.FindFirst("family_name")?.Value ?? "User";
        var role = _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value ?? 
                  _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";

        return await Task.FromResult(new UserResponseDto
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = email ?? "unknown@example.com",
            Role = role
        });
    }
}