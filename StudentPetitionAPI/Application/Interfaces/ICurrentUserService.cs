namespace StudentPetitionAPI.Application.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    string? Email { get; }

    string? Role { get; }

    bool IsStudent { get; }

    bool IsReviewer { get; }
}
