using Microsoft.EntityFrameworkCore;
using StudentPetitionAPI.Infrastructure.Data;
using StudentPetitionAPI.Domain.Entities;
using StudentPetitionAPI.Domain.Enums;
using StudentPetitionAPI.Application.Interfaces;

namespace StudentPetitionAPI.Infrastructure.Repositories;

public class PetitionRepository : GenericRepository<Petition>, IPetitionRepository
{
    public PetitionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public override async Task<Petition?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<Petition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(p => p.Student)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Petition> Items, int TotalCount)> GetFilteredPagedAsync(
        PetitionStatus? status = null,
        PetitionType? petitionType = null,
        int? studentId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Build a single filtered IQueryable; filters compose into one SQL WHERE clause.
        IQueryable<Petition> query = DbSet.AsNoTracking();

        query = ApplyFilters(query, status, petitionType, studentId, dateFrom, dateTo);

        // Count without Include/OrderBy to avoid unnecessary joins and sorting work.
        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return (Array.Empty<Petition>(), 0);
        }

        // Include Student only for the current page — not for the count query.
        var items = await query
            .Include(p => p.Student)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Petition> ApplyFilters(
        IQueryable<Petition> query,
        PetitionStatus? status,
        PetitionType? petitionType,
        int? studentId,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (petitionType.HasValue)
        {
            query = query.Where(p => p.PetitionType == petitionType.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(p => p.StudentId == studentId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= dateTo.Value);
        }

        return query;
    }
}
