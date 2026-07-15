namespace StudentPetitionAPI.Application.DTOs.Responses;

public record StudentResponse
{
    public int Id { get; init; }

    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string StudentNumber { get; init; } = null!;

    public DateTime CreatedAt { get; init; }
}
