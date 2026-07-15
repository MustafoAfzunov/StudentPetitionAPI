using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;

namespace StudentPetitionAPI.Application.Interfaces;

public interface IPetitionService
{
    Task<PetitionResponse> CreateAsync(
        CreatePetitionRequest request,
        CancellationToken cancellationToken = default);

    Task<PetitionResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<PetitionResponse>> GetFilteredAsync(
        ListPetitionsRequest request,
        CancellationToken cancellationToken = default);

    Task<PetitionResponse> UpdateAsync(
        int id,
        UpdatePetitionRequest request,
        CancellationToken cancellationToken = default);

    Task<PetitionResponse> SubmitAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PetitionResponse> ReviewAsync(
        int id,
        ReviewPetitionRequest request,
        CancellationToken cancellationToken = default);
}
