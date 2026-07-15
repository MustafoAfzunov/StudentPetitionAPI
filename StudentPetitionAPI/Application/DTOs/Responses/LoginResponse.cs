namespace StudentPetitionAPI.Application.DTOs.Responses;

public record LoginResponse
{
    public string AccessToken { get; init; } = null!;

    public string TokenType { get; init; } = "Bearer";

    public int ExpiresInMinutes { get; init; }

    public string Email { get; init; } = null!;

    public string Role { get; init; } = null!;
}
