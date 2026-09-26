using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public record CreateIncidentRequest(string Title, string Description, IncidentSeverity Severity);

public record UpdateIncidentStatusRequest(IncidentStatus Status);

public record AssignIncidentRequest(Guid UserId);

public record AddIncidentCommentRequest(string Message);

public record IncidentCommentResponse(Guid Id, Guid UserId, string UserName, string Message, DateTime CreatedAt);

public record IncidentResponse(
    Guid Id,
    string Title,
    string Description,
    IncidentSeverity Severity,
    IncidentStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime SlaDeadline,
    bool IsSlaBreached,
    DateTime? ResolvedAt,
    Guid ReporterId,
    string ReporterName,
    Guid? AssignedToId,
    string? AssignedToName);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record IncidentQueryParameters
{
    public IncidentStatus? Status { get; init; }
    public IncidentSeverity? Severity { get; init; }
    public Guid? AssignedToId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
