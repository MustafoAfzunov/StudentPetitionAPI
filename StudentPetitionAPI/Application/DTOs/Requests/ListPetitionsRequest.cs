using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.DTOs.Requests;

public record ListPetitionsRequest
{
    public PetitionStatus? Status { get; init; }

    public PetitionType? PetitionType { get; init; }

    public int? StudentId { get; init; }

    public DateTime? DateFrom { get; init; }

    public DateTime? DateTo { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
