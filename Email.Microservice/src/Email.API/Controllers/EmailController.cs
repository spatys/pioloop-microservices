using Microsoft.AspNetCore.Mvc;
using Email.Domain.Interfaces;

namespace Email.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-verification")]
    public async Task<IActionResult> SendVerification([FromBody] SendVerificationEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { success = false, message = "Email et code requis" });
        }

        await _emailService.SendEmailConfirmationAsync(request.Email, request.Code);
        return Ok(new { success = true });
    }
}

public record SendVerificationEmailRequest(string Email, string Code);


