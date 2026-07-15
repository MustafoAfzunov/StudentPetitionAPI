using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPetitionAPI.Infrastructure.Authentication;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;
using StudentPetitionAPI.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentPetitionAPI.Controllers;

/// <summary>
/// Student profile management endpoints.
/// </summary>
[ApiController]
[Route("api/students")]
[Produces("application/json")]
[SwaggerTag("Students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Creates a new student profile.
    /// </summary>
    /// <param name="request">Student details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created student.</returns>
    /// <remarks>Requires the <c>Student</c> role. Reviewers are forbidden.</remarks>
    /// <response code="201">Student created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller is not allowed to create students.</response>
    /// <response code="409">Email or student number already exists.</response>
    [HttpPost]
    [Authorize(Policy = Policies.CanCreateStudents)]
    [SwaggerOperation(Summary = "Create student", Description = "Creates a student profile. Policy: CanCreateStudents.")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentResponse>> CreateStudent(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
    }

    /// <summary>
    /// Gets a student by identifier.
    /// </summary>
    /// <param name="id">Student id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Student details.</returns>
    /// <response code="200">Student found.</response>
    /// <response code="404">Student not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.CanViewStudents)]
    [SwaggerOperation(Summary = "Get student by id", Description = "Returns a single student. Policy: CanViewStudents.")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentResponse>> GetStudentById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentService.GetByIdAsync(id, cancellationToken);
        return Ok(student);
    }

    /// <summary>
    /// Returns a paged list of students.
    /// </summary>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Page size (default 10, max 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged student list.</returns>
    /// <response code="200">Paged students returned.</response>
    [HttpGet]
    [Authorize(Policy = Policies.CanViewStudents)]
    [SwaggerOperation(Summary = "List students", Description = "Paged student list. Query: page, pageSize.")]
    [ProducesResponseType(typeof(PagedResponse<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<StudentResponse>>> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _studentService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}
