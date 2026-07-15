using Microsoft.EntityFrameworkCore;
using StudentPetitionAPI.Infrastructure.Data;
using StudentPetitionAPI.Domain.Entities;
using StudentPetitionAPI.Application.Interfaces;

namespace StudentPetitionAPI.Infrastructure.Repositories;

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<bool> StudentNumberExistsAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(s => s.StudentNumber == studentNumber, cancellationToken);
    }

    public async Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = DbSet.AsNoTracking();

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(s => s.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
