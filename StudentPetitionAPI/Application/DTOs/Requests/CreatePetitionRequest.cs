using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.DTOs.Requests;

public record CreatePetitionRequest
{
    public int StudentId { get; init; }

    public PetitionType PetitionType { get; init; }

    public string Title { get; init; } = null!;

    public string Description { get; init; } = null!;
}
