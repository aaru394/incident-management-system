using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Interfaces;

public interface IIncidentService
{
    Task<IncidentResponse> CreateAsync(CreateIncidentRequest request, Guid reporterId, CancellationToken ct = default);
    Task<IncidentResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<IncidentResponse>> QueryAsync(IncidentQueryParameters parameters, CancellationToken ct = default);
    Task<IncidentResponse> AssignAsync(Guid incidentId, AssignIncidentRequest request, Guid changedByUserId, CancellationToken ct = default);
    Task<IncidentResponse> ChangeStatusAsync(Guid incidentId, UpdateIncidentStatusRequest request, Guid changedByUserId, CancellationToken ct = default);
    Task<IncidentCommentResponse> AddCommentAsync(Guid incidentId, AddIncidentCommentRequest request, Guid userId, CancellationToken ct = default);
}
