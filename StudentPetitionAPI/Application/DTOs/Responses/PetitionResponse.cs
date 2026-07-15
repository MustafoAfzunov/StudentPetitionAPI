using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.DTOs.Responses;

public record PetitionResponse
{
    public int Id { get; init; }

    public int StudentId { get; init; }

    public string StudentFirstName { get; init; } = null!;

    public string StudentLastName { get; init; } = null!;

    public string StudentNumber { get; init; } = null!;

    public PetitionType PetitionType { get; init; }

    public string Title { get; init; } = null!;

    public string Description { get; init; } = null!;

    public PetitionStatus Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public string? ReviewedBy { get; init; }

    public DateTime? ReviewedAt { get; init; }

    public string? ReviewComment { get; init; }
}
