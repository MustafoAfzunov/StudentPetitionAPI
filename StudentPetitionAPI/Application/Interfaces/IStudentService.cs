using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;

namespace StudentPetitionAPI.Application.Interfaces;

public interface IStudentService
{
    Task<StudentResponse> CreateAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<StudentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<StudentResponse>> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
