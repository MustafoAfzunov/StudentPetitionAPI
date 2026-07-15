using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.DTOs.Requests;

public record UpdatePetitionRequest
{
    public PetitionType PetitionType { get; init; }

    public string Title { get; init; } = null!;

    public string Description { get; init; } = null!;
}
