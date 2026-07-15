using StudentPetitionAPI.Domain.Entities;

namespace StudentPetitionAPI.Application.Interfaces;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> StudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
