using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;
using StudentPetitionAPI.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace StudentPetitionAPI.Controllers;

/// <summary>
/// Authentication endpoints for issuing JWT access tokens.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[SwaggerTag("Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token, expiry, email, and role.</returns>
    /// <remarks>
    /// Demo users:
    /// - student@test.com / Student123! (Student)
    /// - reviewer@test.com / Reviewer123! (Reviewer)
    /// </remarks>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Invalid email or password.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Validates hardcoded demo credentials and returns a Bearer JWT.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
}
