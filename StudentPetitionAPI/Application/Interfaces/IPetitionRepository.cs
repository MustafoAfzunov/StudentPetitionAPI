using StudentPetitionAPI.Domain.Entities;
using StudentPetitionAPI.Domain.Enums;

namespace StudentPetitionAPI.Application.Interfaces;

public interface IPetitionRepository : IGenericRepository<Petition>
{
    Task<(IReadOnlyList<Petition> Items, int TotalCount)> GetFilteredPagedAsync(
        PetitionStatus? status = null,
        PetitionType? petitionType = null,
        int? studentId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
