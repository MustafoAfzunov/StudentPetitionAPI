namespace StudentPetitionAPI.Application.DTOs.Requests;

public record LoginRequest
{
    public string Email { get; init; } = null!;

    public string Password { get; init; } = null!;
}
