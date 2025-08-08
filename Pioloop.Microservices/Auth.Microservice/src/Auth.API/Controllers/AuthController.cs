using Microsoft.AspNetCore.Mvc;
using MediatR;
using Auth.Application.Commands;
using Auth.Application.Queries;
using Auth.Application.DTOs;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    /// <param name="request">User credentials</param>
    /// <returns>Login response with user data and JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<LoginResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<LoginResponseDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<LoginResponseDto>))]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">User registration data</param>
    /// <returns>Registration response with user data</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<UserDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<UserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<UserDto>))]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            PhoneNumber = request.PhoneNumber,
            AcceptConsent = request.AcceptConsent
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Verify email with verification code
    /// </summary>
    /// <param name="request">Email verification data</param>
    /// <returns>Verification result</returns>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<bool>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<bool>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<bool>))]
    public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequestDto request)
    {
        var command = new VerifyEmailCommand
        {
            Email = request.Email,
            Code = request.Code
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User data</returns>
    [HttpGet("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<UserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<UserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<UserDto>))]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return NotFound(result);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User data</returns>
    [HttpGet("users/email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<UserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<UserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<UserDto>))]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery { Email = email };
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return NotFound(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
