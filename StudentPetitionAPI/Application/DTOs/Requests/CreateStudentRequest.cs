namespace StudentPetitionAPI.Application.DTOs.Requests;

public record CreateStudentRequest
{
    public string FirstName { get; init; } = null!;

    public string LastName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string StudentNumber { get; init; } = null!;
}
