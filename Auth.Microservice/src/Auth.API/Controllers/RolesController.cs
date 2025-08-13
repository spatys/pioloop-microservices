using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Auth.Domain.Identity;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RolesController(RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return BadRequest("Role name required");
        if (await _roleManager.RoleExistsAsync(roleName)) return Conflict("Role already exists");
        var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        if (!result.Succeeded) return StatusCode(500, string.Join(";", result.Errors.Select(e => e.Description)));
        return Ok(new { success = true, roleName });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromQuery] Guid userId, [FromQuery] string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound("User not found");
        if (!await _roleManager.RoleExistsAsync(roleName)) return NotFound("Role not found");
        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded) return StatusCode(500, string.Join(";", result.Errors.Select(e => e.Description)));
        return Ok(new { success = true });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveRole([FromQuery] Guid userId, [FromQuery] string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound("User not found");
        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded) return StatusCode(500, string.Join(";", result.Errors.Select(e => e.Description)));
        return Ok(new { success = true });
    }
}


