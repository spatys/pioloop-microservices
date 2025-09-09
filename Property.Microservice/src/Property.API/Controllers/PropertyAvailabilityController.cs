using Microsoft.AspNetCore.Mvc;
using MediatR;
using Property.Application.Commands;
using Property.Application.Queries;
using Property.Application.DTOs.Request;
using Property.Application.DTOs.Response;
using Microsoft.AspNetCore.Authorization;

namespace Property.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PropertyAvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyAvailabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get availability for a property
    /// </summary>
    [HttpGet("property/{propertyId}")]
    public async Task<ActionResult<List<PropertyAvailabilityResponse>>> GetPropertyAvailability(
        Guid propertyId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetPropertyAvailabilityQuery
        {
            Request = new GetAvailabilityRequest
            {
                PropertyId = propertyId,
                StartDate = startDate,
                EndDate = endDate
            }
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get availability calendar for a property
    /// </summary>
    [HttpGet("property/{propertyId}/calendar")]
    public async Task<ActionResult<PropertyAvailabilityCalendarResponse>> GetAvailabilityCalendar(
        Guid propertyId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetPropertyAvailabilityCalendarQuery
        {
            PropertyId = propertyId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new availability period
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PropertyAvailabilityResponse>> CreateAvailability(
        [FromBody] CreatePropertyAvailabilityRequest request)
    {
        var command = new CreatePropertyAvailabilityCommand
        {
            Request = request
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAvailabilityById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing availability period
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<PropertyAvailabilityResponse>> UpdateAvailability(
        Guid id,
        [FromBody] UpdatePropertyAvailabilityRequest request)
    {
        request.Id = id;
        var command = new UpdatePropertyAvailabilityCommand
        {
            Request = request
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete an availability period
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAvailability(Guid id)
    {
        var command = new DeletePropertyAvailabilityCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Bulk update availability periods
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<List<PropertyAvailabilityResponse>>> BulkUpdateAvailability(
        [FromBody] BulkUpdateAvailabilityRequest request)
    {
        var command = new BulkUpdateAvailabilityCommand
        {
            Request = request
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific availability by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PropertyAvailabilityResponse>> GetAvailabilityById(Guid id)
    {
        // This would need a separate query handler
        return Ok();
    }
}
