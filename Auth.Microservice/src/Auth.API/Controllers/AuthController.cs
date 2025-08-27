using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Auth.Application.Commands;
using Auth.Application.Queries;
using Auth.Application.DTOs.Request;
using Auth.Application.DTOs.Response;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IHostEnvironment env, IConfiguration configuration)
    {
        _mediator = mediator;
        _env = env;
        _configuration = configuration;
    }

    /// <summary>
    /// Step 1: Initiate registration with email only
    /// Creates a temporary user and sends verification code via email
    /// </summary>
    /// <param name="request">Email address for registration</param>
    /// <returns>Registration initiated successfully with confirmation message</returns>
    [HttpPost("register/register-email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<RegisterEmailResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<RegisterEmailResponseDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<RegisterEmailResponseDto>))]
    public async Task<IActionResult> RegisterEmail([FromBody] RegisterEmailRequest request)
    {
        var command = new RegisterEmailCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Step 2: Verify email code
    /// Validates the 6-digit code sent to user's email and confirms email
    /// </summary>
    /// <param name="request">Email and verification code</param>
    /// <returns>Email verified successfully with verification status</returns>
    [HttpPost("register/register-verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<RegisterVerifyEmailResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<RegisterVerifyEmailResponseDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<RegisterVerifyEmailResponseDto>))]
    public async Task<IActionResult> RegisterVerifyEmail([FromBody] RegisterVerifyEmailRequest request)
    {
        var command = new RegisterVerifyEmailCommand
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
    /// Resend email verification code
    /// Sends a new verification code to user's email if the previous one expired or was lost
    /// </summary>
    /// <param name="request">Email address</param>
    /// <returns>New verification code sent successfully</returns>
    [HttpPost("register/resend-email-code")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<ResendEmailCodeResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<ResendEmailCodeResponseDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<ResendEmailCodeResponseDto>))]
    public async Task<IActionResult> ResendEmailCode([FromBody] ResendEmailCodeRequest request)
    {
        var command = new ResendEmailCodeCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Step 3: Complete registration with profile details
    /// Finalizes user registration with personal information and password
    /// Requires email to be verified from Step 2
    /// </summary>
    /// <param name="request">Complete user profile information</param>
    /// <returns>Registration completed successfully with user data</returns>
    [HttpPost("register/register-complete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<LoginResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<LoginResponseDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<LoginResponseDto>))]
    public async Task<IActionResult> RegisterComplete([FromBody] RegisterCompleteRequest request)
    {
        var command = new RegisterCompleteCommand
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            // Poser le cookie HttpOnly et vider le token (le handler a généré le token)
            if (result.Data is not null && !string.IsNullOrWhiteSpace(result.Data.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _env.IsProduction(),
                    SameSite = SameSiteMode.Lax,
                    Expires = result.Data.ExpiresAt
                };
                Response.Cookies.Append("auth_token", result.Data.Token, cookieOptions);
                result.Data.Token = string.Empty;
            }
            return Ok(result);
        }
        
        return BadRequest(result);
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
            // Poser le cookie HttpOnly côté backend et vider le token du body
            if (result.Data is not null && !string.IsNullOrWhiteSpace(result.Data.Token))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _env.IsProduction(),
                    SameSite = SameSiteMode.Lax,
                    Expires = result.Data.ExpiresAt
                };
                Response.Cookies.Append("auth_token", result.Data.Token, cookieOptions);
                result.Data.Token = string.Empty;
            }
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
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
    /// Get current user information
    /// </summary>
    /// <returns>Current user data</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<ApplicationUserDto>))]
    public async Task<IActionResult> GetCurrentUser()
    {
        var query = new GetCurrentUserQuery();
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return Unauthorized(result);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<object>))]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Password change confirmation</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<object>))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmNewPassword = request.ConfirmNewPassword
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Password reset email sent confirmation</returns>
    [HttpPost("request-password-reset")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<object>))]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        var command = new RequestPasswordResetCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Password reset with token request</param>
    /// <returns>Password reset confirmation</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<object>))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand
        {
            Email = request.Email,
            Token = request.Token,
            NewPassword = request.NewPassword,
            ConfirmNewPassword = request.ConfirmNewPassword
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Update user role
    /// Updates the role of a specific user (internal use by other microservices)
    /// </summary>
    /// <param name="request">User ID and new role</param>
    /// <returns>Role updated successfully</returns>
    [HttpPost("update-role")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponseDto<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponseDto<object>))]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
    {
        var command = new UpdateUserRoleCommand
        {
            UserId = request.UserId,
            NewRole = request.NewRole
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }
        
        return BadRequest(result);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
