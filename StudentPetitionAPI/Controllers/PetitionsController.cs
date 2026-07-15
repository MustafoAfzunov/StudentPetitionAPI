using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPetitionAPI.Infrastructure.Authentication;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;
using StudentPetitionAPI.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentPetitionAPI.Controllers;

/// <summary>
/// Petition submission, listing, update, submit, and review endpoints.
/// </summary>
[ApiController]
[Route("api/petitions")]
[Produces("application/json")]
[Authorize]
[SwaggerTag("Petitions")]
public class PetitionsController : ControllerBase
{
    private readonly IPetitionService _petitionService;

    public PetitionsController(IPetitionService petitionService)
    {
        _petitionService = petitionService;
    }

    /// <summary>
    /// Creates a new petition in <c>Draft</c> status.
    /// </summary>
    /// <param name="request">Petition details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created petition.</returns>
    /// <remarks>Requires the <c>Student</c> role. Student may only create petitions for their own profile email.</remarks>
    /// <response code="201">Petition created as Draft.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Caller cannot create petitions for this student.</response>
    /// <response code="404">Target student not found.</response>
    [HttpPost]
    [Authorize(Policy = Policies.CanCreatePetitions)]
    [SwaggerOperation(Summary = "Create petition", Description = "Creates a Draft petition. Policy: CanCreatePetitions.")]
    [ProducesResponseType(typeof(PetitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetitionResponse>> CreatePetition(
        [FromBody] CreatePetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPetitionById), new { id = petition.Id }, petition);
    }

    /// <summary>
    /// Gets a petition by identifier.
    /// </summary>
    /// <param name="id">Petition id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Petition details.</returns>
    /// <remarks>Students may only view their own petitions. Reviewers may view any petition.</remarks>
    /// <response code="200">Petition found.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">Petition not found.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.CanViewPetitions)]
    [SwaggerOperation(Summary = "Get petition by id", Description = "Returns a petition. Policy: CanViewPetitions.")]
    [ProducesResponseType(typeof(PetitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetitionResponse>> GetPetitionById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionService.GetByIdAsync(id, cancellationToken);
        return Ok(petition);
    }

    /// <summary>
    /// Returns a filtered, paged list of petitions.
    /// </summary>
    /// <param name="request">Optional filters and paging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged petition list.</returns>
    /// <remarks>
    /// Filters: status, petitionType, studentId, dateFrom, dateTo, page, pageSize.
    /// Students are automatically scoped to their own petitions.
    /// </remarks>
    /// <response code="200">Paged petitions returned.</response>
    [HttpGet]
    [Authorize(Policy = Policies.CanViewPetitions)]
    [SwaggerOperation(
        Summary = "List petitions",
        Description = "Filtered paged list. Query: status, petitionType, studentId, dateFrom, dateTo, page, pageSize.")]
    [ProducesResponseType(typeof(PagedResponse<PetitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PetitionResponse>>> GetPetitions(
        [FromQuery] ListPetitionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _petitionService.GetFilteredAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates a petition that is still in <c>Draft</c> status.
    /// </summary>
    /// <param name="id">Petition id.</param>
    /// <param name="request">Updated petition content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated petition.</returns>
    /// <response code="200">Petition updated.</response>
    /// <response code="400">Invalid status transition or validation error.</response>
    /// <response code="403">Caller does not own the petition.</response>
    /// <response code="404">Petition not found.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.CanManageOwnPetitions)]
    [SwaggerOperation(Summary = "Update petition", Description = "Updates Draft petitions only. Policy: CanManageOwnPetitions.")]
    [ProducesResponseType(typeof(PetitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetitionResponse>> UpdatePetition(
        int id,
        [FromBody] UpdatePetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionService.UpdateAsync(id, request, cancellationToken);
        return Ok(petition);
    }

    /// <summary>
    /// Submits a draft petition for review (<c>Draft</c> → <c>Submitted</c>).
    /// </summary>
    /// <param name="id">Petition id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Submitted petition.</returns>
    /// <response code="200">Petition submitted.</response>
    /// <response code="403">Caller does not own the petition.</response>
    /// <response code="404">Petition not found.</response>
    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = Policies.CanManageOwnPetitions)]
    [SwaggerOperation(Summary = "Submit petition", Description = "Draft → Submitted. Policy: CanManageOwnPetitions.")]
    [ProducesResponseType(typeof(PetitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetitionResponse>> SubmitPetition(
        int id,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionService.SubmitAsync(id, cancellationToken);
        return Ok(petition);
    }

    /// <summary>
    /// Reviews a submitted petition (<c>Submitted</c> → <c>UnderReview</c> → <c>Approved</c>|<c>Rejected</c>).
    /// </summary>
    /// <param name="id">Petition id.</param>
    /// <param name="request">Review decision and comment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reviewed petition.</returns>
    /// <remarks>Requires the <c>Reviewer</c> role. <c>ReviewComment</c> and <c>ReviewedBy</c> are mandatory.</remarks>
    /// <response code="200">Petition reviewed.</response>
    /// <response code="400">Invalid review payload or status transition.</response>
    /// <response code="403">Caller is not a reviewer.</response>
    /// <response code="404">Petition not found.</response>
    [HttpPost("{id:int}/review")]
    [Authorize(Policy = Policies.CanReviewPetitions)]
    [SwaggerOperation(
        Summary = "Review petition",
        Description = "Submitted → UnderReview → Approved|Rejected. Policy: CanReviewPetitions.")]
    [ProducesResponseType(typeof(PetitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetitionResponse>> ReviewPetition(
        int id,
        [FromBody] ReviewPetitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var petition = await _petitionService.ReviewAsync(id, request, cancellationToken);
        return Ok(petition);
    }
}
