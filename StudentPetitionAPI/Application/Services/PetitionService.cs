using AutoMapper;
using StudentPetitionAPI.Domain.Enums;
using StudentPetitionAPI.Application.Exceptions;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;
using StudentPetitionAPI.Application.Interfaces;
using StudentPetitionAPI.Domain.Entities;

namespace StudentPetitionAPI.Application.Services;

public class PetitionService : IPetitionService
{
    private readonly IPetitionRepository _petitionRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private int? _cachedCurrentStudentId;

    public PetitionService(
        IPetitionRepository petitionRepository,
        IStudentRepository studentRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _petitionRepository = petitionRepository;
        _studentRepository = studentRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PetitionResponse> CreateAsync(
        CreatePetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken)
            ?? throw new NotFoundException($"Student with id '{request.StudentId}' was not found.");

        EnsureStudentOwnsEmail(student.Email);

        var now = DateTime.UtcNow;
        var petition = _mapper.Map<Petition>(request);

        petition.Status = PetitionStatus.Draft;
        petition.CreatedAt = now;
        petition.UpdatedAt = now;
        petition.ReviewedBy = null;
        petition.ReviewedAt = null;
        petition.ReviewComment = null;
        petition.Student = student;

        await _petitionRepository.AddAsync(petition, cancellationToken);
        await _petitionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PetitionResponse>(petition);
    }

    public async Task<PetitionResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Petition with id '{id}' was not found.");

        EnsureCanAccessPetition(petition.Student.Email);

        return _mapper.Map<PetitionResponse>(petition);
    }

    public async Task<PagedResponse<PetitionResponse>> GetFilteredAsync(
        ListPetitionsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DateFrom.HasValue
            && request.DateTo.HasValue
            && request.DateFrom > request.DateTo)
        {
            throw new BusinessRuleException("'dateFrom' cannot be later than 'dateTo'.");
        }

        const int maxPageSize = 100;
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, maxPageSize);

        var studentIdFilter = request.StudentId;

        if (_currentUser.IsStudent)
        {
            var ownStudentId = await GetCurrentStudentIdAsync(cancellationToken);
            if (studentIdFilter.HasValue && studentIdFilter.Value != ownStudentId)
            {
                throw new ForbiddenException("Students can only view their own petitions.");
            }

            studentIdFilter = ownStudentId;
        }

        var (items, totalCount) = await _petitionRepository.GetFilteredPagedAsync(
            status: request.Status,
            petitionType: request.PetitionType,
            studentId: studentIdFilter,
            dateFrom: request.DateFrom,
            dateTo: request.DateTo,
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        return new PagedResponse<PetitionResponse>
        {
            Items = _mapper.Map<IReadOnlyList<PetitionResponse>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PetitionResponse> UpdateAsync(
        int id,
        UpdatePetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Petition with id '{id}' was not found.");

        EnsureStudentOwnsEmail(petition.Student.Email);
        PetitionStatusTransitions.EnsureCurrent(petition.Status, PetitionStatus.Draft, action: "update");

        _mapper.Map(request, petition);
        petition.Status = PetitionStatus.Draft;
        petition.UpdatedAt = DateTime.UtcNow;

        // Entity is already tracked by GetByIdAsync — no DbSet.Update needed.
        await _petitionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PetitionResponse>(petition);
    }

    public async Task<PetitionResponse> SubmitAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Petition with id '{id}' was not found.");

        EnsureStudentOwnsEmail(petition.Student.Email);
        PetitionStatusTransitions.Ensure(petition.Status, PetitionStatus.Submitted, action: "submit");

        petition.Status = PetitionStatus.Submitted;
        petition.UpdatedAt = DateTime.UtcNow;

        await _petitionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PetitionResponse>(petition);
    }

    public async Task<PetitionResponse> ReviewAsync(
        int id,
        ReviewPetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Petition with id '{id}' was not found.");

        ValidateReviewRequest(request);

        if (petition.Status is PetitionStatus.Approved or PetitionStatus.Rejected)
        {
            throw new InvalidStatusTransitionException(
                action: "review",
                currentStatus: petition.Status,
                attemptedStatus: request.Status);
        }

        if (petition.Status == PetitionStatus.Draft)
        {
            throw new InvalidStatusTransitionException(
                action: "review",
                currentStatus: PetitionStatus.Draft,
                attemptedStatus: request.Status);
        }

        var now = DateTime.UtcNow;

        if (petition.Status == PetitionStatus.Submitted)
        {
            PetitionStatusTransitions.Ensure(
                PetitionStatus.Submitted,
                PetitionStatus.UnderReview,
                action: "take into review");
            petition.Status = PetitionStatus.UnderReview;
            petition.UpdatedAt = now;
        }

        if (petition.Status != PetitionStatus.UnderReview)
        {
            throw new InvalidStatusTransitionException(
                action: "review",
                currentStatus: petition.Status,
                attemptedStatus: request.Status);
        }

        PetitionStatusTransitions.Ensure(PetitionStatus.UnderReview, request.Status, action: "review");

        petition.Status = request.Status;
        petition.ReviewedBy = string.IsNullOrWhiteSpace(request.ReviewedBy)
            ? _currentUser.Email ?? request.ReviewedBy
            : request.ReviewedBy.Trim();
        petition.ReviewComment = request.ReviewComment.Trim();
        petition.ReviewedAt = now;
        petition.UpdatedAt = now;

        await _petitionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PetitionResponse>(petition);
    }

    private void EnsureCanAccessPetition(string petitionStudentEmail)
    {
        if (_currentUser.IsReviewer)
        {
            return;
        }

        EnsureStudentOwnsEmail(petitionStudentEmail);
    }

    private void EnsureStudentOwnsEmail(string resourceEmail)
    {
        if (!_currentUser.IsStudent)
        {
            throw new ForbiddenException("Only students can perform this action on their own petitions.");
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Email)
            || !string.Equals(_currentUser.Email, resourceEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Students can only access their own petitions.");
        }
    }

    private async Task<int> GetCurrentStudentIdAsync(CancellationToken cancellationToken)
    {
        if (_cachedCurrentStudentId.HasValue)
        {
            return _cachedCurrentStudentId.Value;
        }

        if (string.IsNullOrWhiteSpace(_currentUser.Email))
        {
            throw new ForbiddenException("Authenticated student email claim is missing.");
        }

        var student = await _studentRepository.GetByEmailAsync(_currentUser.Email, cancellationToken)
            ?? throw new ForbiddenException(
                $"No student profile is linked to '{_currentUser.Email}'. Create a student with this email first.");

        _cachedCurrentStudentId = student.Id;
        return student.Id;
    }

    private static void ValidateReviewRequest(ReviewPetitionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReviewComment))
        {
            throw new BusinessRuleException("ReviewComment is mandatory when reviewing a petition.");
        }

        if (string.IsNullOrWhiteSpace(request.ReviewedBy))
        {
            throw new BusinessRuleException("ReviewedBy is mandatory when reviewing a petition.");
        }

        if (request.Status is not (PetitionStatus.Approved or PetitionStatus.Rejected))
        {
            throw new InvalidStatusTransitionException(
                action: "review",
                currentStatus: PetitionStatus.UnderReview,
                attemptedStatus: request.Status);
        }
    }
}
