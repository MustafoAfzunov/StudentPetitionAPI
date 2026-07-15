using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.DTOs.Requests;

public record ReviewPetitionRequest
{
    public PetitionStatus Status { get; init; }

    public string ReviewedBy { get; init; } = null!;

    public string ReviewComment { get; init; } = null!;
}
