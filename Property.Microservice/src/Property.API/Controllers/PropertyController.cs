using MediatR;
using Microsoft.AspNetCore.Mvc;
using Property.Application.Commands;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Application.Queries;
using System.Security.Claims;

namespace Property.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search properties with filters and pagination
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PropertySearchResponse>> Search([FromQuery] PropertySearchCriteriaRequest searchCriteria)
    {
        var result = await _mediator.Send(new SearchPropertiesQuery(searchCriteria));
        return Ok(result);
    }

    /// <summary>
    /// Get property by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyResponse>> GetById(Guid id)
    {
        var property = await _mediator.Send(new GetPropertyByIdQuery(id));
        if (property == null)
        {
            return NotFound();
        }
        return Ok(property);
    }

    /// <summary>
    /// Get properties by owner ID
    /// </summary>
    [HttpGet("owner/{ownerId:guid}")]
    public async Task<ActionResult<IEnumerable<PropertyResponse>>> GetByOwnerId(Guid ownerId)
    {
        // Vérifier que l'utilisateur authentifié accède à ses propres propriétés
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || 
            !Guid.TryParse(userIdHeader.FirstOrDefault(), out var userId) ||
            userId != ownerId)
        {
            return Unauthorized("Accès non autorisé");
        }

        var properties = await _mediator.Send(new GetPropertiesByOwnerIdQuery(ownerId));
        return Ok(properties);
    }

    /// <summary>
    /// Create a new property (specific endpoint)
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<PropertyResponse>> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        try
        {
            // Récupérer l'ID utilisateur depuis le header X-User-Id injecté par l'API Gateway
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || 
                !Guid.TryParse(userIdHeader.FirstOrDefault(), out var userId))
            {
                return Unauthorized("Utilisateur non authentifié");
            }

            var command = new CreatePropertyCommand(request, userId);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erreur lors de la création de la propriété : {ex.Message}");
        }
    }

    /// <summary>
    /// Get properties for the authenticated user
    /// </summary>
    [HttpGet("my-properties")]
    public async Task<ActionResult<IEnumerable<PropertyResponse>>> GetMyProperties()
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || 
            !Guid.TryParse(userIdHeader.FirstOrDefault(), out var userId))
        {
            return Unauthorized("Utilisateur non authentifié");
        }

        var properties = await _mediator.Send(new GetPropertiesByOwnerIdQuery(userId));
        return Ok(properties);
    }

    /// <summary>
    /// Update an existing property
    /// </summary>
    [HttpPut("update/{id}")]
    public async Task<ActionResult<PropertyResponse>> Update(Guid id, [FromBody] UpdatePropertyRequest updatePropertyRequest)
    {
        var property = await _mediator.Send(new UpdatePropertyCommand(id, updatePropertyRequest));
        return Ok(property);
    }




}
