using FluentValidation;
using IncidentManagement.Api.Extensions;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentsController(
    IIncidentService incidentService,
    IValidator<CreateIncidentRequest> createValidator,
    IValidator<AddIncidentCommentRequest> commentValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<IncidentResponse>> Create(CreateIncidentRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);
        var result = await incidentService.CreateAsync(request, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await incidentService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IncidentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<IncidentResponse>>> Query(
        [FromQuery] IncidentStatus? status,
        [FromQuery] IncidentSeverity? severity,
        [FromQuery] Guid? assignedToId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var parameters = new IncidentQueryParameters
        {
            Status = status,
            Severity = severity,
            AssignedToId = assignedToId,
            Page = page,
            PageSize = Math.Clamp(pageSize, 1, 100)
        };
        var result = await incidentService.QueryAsync(parameters, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Engineer,Admin")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentResponse>> Assign(Guid id, AssignIncidentRequest request, CancellationToken ct)
    {
        var result = await incidentService.AssignAsync(id, request, User.GetUserId(), ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Engineer,Admin")]
    [ProducesResponseType(typeof(IncidentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentResponse>> ChangeStatus(Guid id, UpdateIncidentStatusRequest request, CancellationToken ct)
    {
        var result = await incidentService.ChangeStatusAsync(id, request, User.GetUserId(), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(IncidentCommentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<IncidentCommentResponse>> AddComment(Guid id, AddIncidentCommentRequest request, CancellationToken ct)
    {
        await commentValidator.ValidateAndThrowAsync(request, ct);
        var result = await incidentService.AddCommentAsync(id, request, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }
}
