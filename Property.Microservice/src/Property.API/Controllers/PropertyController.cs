using MediatR;
using Microsoft.AspNetCore.Mvc;
using Property.Application.Commands;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Property.Application.Queries;

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
    /// Create a new property
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PropertyResponse>> Create([FromBody] CreatePropertyRequest createPropertyRequest)
    {
        var property = await _mediator.Send(new CreatePropertyCommand(createPropertyRequest));
        return CreatedAtAction(nameof(GetById), new { id = property.Id }, property);
    }

    /// <summary>
    /// Update an existing property
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PropertyResponse>> Update(Guid id, [FromBody] UpdatePropertyRequest updatePropertyRequest)
    {
                    var property = await _mediator.Send(new UpdatePropertyCommand(id, updatePropertyRequest));
        return Ok(property);
    }


}
