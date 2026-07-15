using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;

namespace StudentPetitionAPI.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
