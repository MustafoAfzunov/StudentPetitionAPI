using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentPetitionAPI.Infrastructure.Authentication;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;
using StudentPetitionAPI.Application.Interfaces;

namespace StudentPetitionAPI.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private static readonly IReadOnlyList<HardcodedUser> Users =
    [
        new("student@test.com", "Student123!", Roles.Student),
        new("reviewer@test.com", "Reviewer123!", Roles.Reviewer)
    ];

    private readonly JwtSettings _jwtSettings;

    public AuthService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = Users.FirstOrDefault(u =>
            string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && u.Password == request.Password);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var token = GenerateToken(user, expires);

        return Task.FromResult(new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresInMinutes = _jwtSettings.ExpirationMinutes,
            Email = user.Email,
            Role = user.Role
        });
    }

    private string GenerateToken(HardcodedUser user, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record HardcodedUser(string Email, string Password, string Role);
}
