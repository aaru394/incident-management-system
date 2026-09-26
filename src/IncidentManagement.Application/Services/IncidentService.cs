using IncidentManagement.Application.Common;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IncidentManagement.Application.Services;

public class IncidentService(
    IIncidentRepository incidentRepository,
    IUserRepository userRepository,
    ICacheService cache,
    ILogger<IncidentService> logger) : IIncidentService
{
    private const string CacheKeyPrefix = "incident:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, Guid reporterId, CancellationToken ct = default)
    {
        var reporter = await userRepository.GetByIdAsync(reporterId, ct)
            ?? throw new NotFoundException(nameof(User), reporterId);

        var incident = Incident.Create(request.Title, request.Description, request.Severity, reporterId);
        await incidentRepository.AddAsync(incident, ct);
        await incidentRepository.SaveChangesAsync(ct);

        logger.LogInformation("Incident {IncidentId} created with severity {Severity} by {ReporterId}", incident.Id, incident.Severity, reporterId);

        return Map(incident, reporter, null);
    }

    public async Task<IncidentResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        var cached = await cache.GetAsync<IncidentResponse>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var incident = await incidentRepository.GetByIdWithDetailsAsync(id, ct);
        if (incident is null)
        {
            return null;
        }

        var response = Map(incident, incident.Reporter, incident.AssignedTo);
        await cache.SetAsync(cacheKey, response, CacheDuration, ct);
        return response;
    }

    public async Task<PagedResult<IncidentResponse>> QueryAsync(IncidentQueryParameters parameters, CancellationToken ct = default)
    {
        var (items, totalCount) = await incidentRepository.QueryAsync(parameters, ct);
        var mapped = items.Select(i => Map(i, i.Reporter, i.AssignedTo)).ToList();
        return new PagedResult<IncidentResponse>(mapped, parameters.Page, parameters.PageSize, totalCount);
    }

    public async Task<IncidentResponse> AssignAsync(Guid incidentId, AssignIncidentRequest request, Guid changedByUserId, CancellationToken ct = default)
    {
        var incident = await incidentRepository.GetByIdWithDetailsAsync(incidentId, ct)
            ?? throw new NotFoundException(nameof(Incident), incidentId);

        var assignee = await userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        incident.AssignTo(request.UserId, changedByUserId);
        incidentRepository.Update(incident);
        await incidentRepository.SaveChangesAsync(ct);
        await InvalidateAsync(incidentId, ct);

        return Map(incident, incident.Reporter, assignee);
    }

    public async Task<IncidentResponse> ChangeStatusAsync(Guid incidentId, UpdateIncidentStatusRequest request, Guid changedByUserId, CancellationToken ct = default)
    {
        var incident = await incidentRepository.GetByIdWithDetailsAsync(incidentId, ct)
            ?? throw new NotFoundException(nameof(Incident), incidentId);

        incident.ChangeStatus(request.Status, changedByUserId);
        incidentRepository.Update(incident);
        await incidentRepository.SaveChangesAsync(ct);
        await InvalidateAsync(incidentId, ct);

        logger.LogInformation("Incident {IncidentId} transitioned to {Status} by {UserId}", incidentId, request.Status, changedByUserId);

        return Map(incident, incident.Reporter, incident.AssignedTo);
    }

    public async Task<IncidentCommentResponse> AddCommentAsync(Guid incidentId, AddIncidentCommentRequest request, Guid userId, CancellationToken ct = default)
    {
        var incident = await incidentRepository.GetByIdAsync(incidentId, ct)
            ?? throw new NotFoundException(nameof(Incident), incidentId);

        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var comment = new IncidentComment
        {
            IncidentId = incidentId,
            UserId = userId,
            Message = request.Message
        };

        incident.Comments.Add(comment);
        incidentRepository.Update(incident);
        await incidentRepository.SaveChangesAsync(ct);
        await InvalidateAsync(incidentId, ct);

        return new IncidentCommentResponse(comment.Id, user.Id, user.FullName, comment.Message, comment.CreatedAt);
    }

    private Task InvalidateAsync(Guid incidentId, CancellationToken ct) =>
        cache.RemoveAsync($"{CacheKeyPrefix}{incidentId}", ct);

    private static IncidentResponse Map(Incident incident, User? reporter, User? assignedTo) => new(
        incident.Id,
        incident.Title,
        incident.Description,
        incident.Severity,
        incident.Status,
        incident.CreatedAt,
        incident.UpdatedAt,
        incident.SlaDeadline,
        incident.IsSlaBreached,
        incident.ResolvedAt,
        incident.ReporterId,
        reporter?.FullName ?? string.Empty,
        incident.AssignedToId,
        assignedTo?.FullName);
}
