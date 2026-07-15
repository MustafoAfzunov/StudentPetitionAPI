using AutoMapper;
using StudentPetitionAPI.Application.Exceptions;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;
using StudentPetitionAPI.Application.Interfaces;
using StudentPetitionAPI.Domain.Entities;

namespace StudentPetitionAPI.Application.Services;

public class StudentService : IStudentService
{
    private const int MaxPageSize = 100;

    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public StudentService(
        IStudentRepository studentRepository,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<StudentResponse> CreateAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsReviewer)
        {
            throw new ForbiddenException("Reviewers cannot create students.");
        }

        if (await _studentRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException($"A student with email '{request.Email}' already exists.");
        }

        if (await _studentRepository.StudentNumberExistsAsync(request.StudentNumber, cancellationToken))
        {
            throw new ConflictException($"A student with number '{request.StudentNumber}' already exists.");
        }

        var student = _mapper.Map<Student>(request);
        student.CreatedAt = DateTime.UtcNow;

        await _studentRepository.AddAsync(student, cancellationToken);
        await _studentRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StudentResponse>(student);
    }

    public async Task<StudentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student with id '{id}' was not found.");

        return _mapper.Map<StudentResponse>(student);
    }

    public async Task<PagedResponse<StudentResponse>> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

        var (items, totalCount) = await _studentRepository.GetPagedAsync(
            page,
            pageSize,
            cancellationToken);

        return new PagedResponse<StudentResponse>
        {
            Items = _mapper.Map<IReadOnlyList<StudentResponse>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
