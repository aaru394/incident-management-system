using IncidentManagement.Domain.Common;
using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class Incident : BaseEntity
{
    private static readonly Dictionary<IncidentStatus, IncidentStatus[]> AllowedTransitions = new()
    {
        [IncidentStatus.Open] = [IncidentStatus.InProgress],
        [IncidentStatus.InProgress] = [IncidentStatus.Resolved, IncidentStatus.Open],
        [IncidentStatus.Resolved] = [IncidentStatus.Closed, IncidentStatus.InProgress],
        [IncidentStatus.Closed] = []
    };

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentStatus Status { get; private set; } = IncidentStatus.Open;
    public DateTime SlaDeadline { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Guid ReporterId { get; set; }
    public User? Reporter { get; set; }

    public Guid? AssignedToId { get; private set; }
    public User? AssignedTo { get; set; }

    public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();
    public ICollection<IncidentStatusChange> StatusHistory { get; set; } = new List<IncidentStatusChange>();

    public bool IsSlaBreached => Status is not (IncidentStatus.Resolved or IncidentStatus.Closed)
        && DateTime.UtcNow > SlaDeadline;

    public static Incident Create(string title, string description, IncidentSeverity severity, Guid reporterId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Incident title is required.");
        }

        var incident = new Incident
        {
            Title = title,
            Description = description,
            Severity = severity,
            ReporterId = reporterId
        };
        incident.SlaDeadline = incident.CreatedAt.Add(SlaWindowFor(severity));
        return incident;
    }

    public void AssignTo(Guid userId, Guid changedByUserId)
    {
        if (Status == IncidentStatus.Closed)
        {
            throw new DomainException("Cannot assign a closed incident.");
        }

        AssignedToId = userId;
        UpdatedAt = DateTime.UtcNow;

        if (Status == IncidentStatus.Open)
        {
            ChangeStatus(IncidentStatus.InProgress, changedByUserId);
        }
    }

    public void ChangeStatus(IncidentStatus newStatus, Guid changedByUserId)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
        {
            throw new DomainException($"Cannot transition incident from {Status} to {newStatus}.");
        }

        StatusHistory.Add(new IncidentStatusChange
        {
            IncidentId = Id,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedByUserId = changedByUserId
        });

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        ResolvedAt = newStatus == IncidentStatus.Resolved ? DateTime.UtcNow : ResolvedAt;
    }

    private static TimeSpan SlaWindowFor(IncidentSeverity severity) => severity switch
    {
        IncidentSeverity.Sev1 => TimeSpan.FromHours(4),
        IncidentSeverity.Sev2 => TimeSpan.FromHours(24),
        IncidentSeverity.Sev3 => TimeSpan.FromHours(72),
        _ => TimeSpan.FromHours(72)
    };
}
