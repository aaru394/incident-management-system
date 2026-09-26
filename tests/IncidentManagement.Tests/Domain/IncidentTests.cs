using IncidentManagement.Domain.Common;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using Xunit;

namespace IncidentManagement.Tests.Domain;

public class IncidentTests
{
    [Fact]
    public void Create_SetsSlaDeadline_BasedOnSeverity()
    {
        var reporterId = Guid.NewGuid();
        var incident = Incident.Create("DB latency spike", "p99 up 3x", IncidentSeverity.Sev1, reporterId);

        Assert.Equal(IncidentStatus.Open, incident.Status);
        Assert.True(incident.SlaDeadline > incident.CreatedAt);
        Assert.True(incident.SlaDeadline <= incident.CreatedAt.AddHours(4).AddSeconds(1));
    }

    [Fact]
    public void Create_ThrowsDomainException_WhenTitleMissing()
    {
        Assert.Throws<DomainException>(() =>
            Incident.Create(" ", "description", IncidentSeverity.Sev2, Guid.NewGuid()));
    }

    [Fact]
    public void ChangeStatus_FromOpenToInProgress_Succeeds()
    {
        var incident = Incident.Create("Payment API down", "5xx errors", IncidentSeverity.Sev1, Guid.NewGuid());
        var actorId = Guid.NewGuid();

        incident.ChangeStatus(IncidentStatus.InProgress, actorId);

        Assert.Equal(IncidentStatus.InProgress, incident.Status);
        Assert.Single(incident.StatusHistory);
        Assert.Equal(IncidentStatus.Open, incident.StatusHistory.First().FromStatus);
        Assert.Equal(IncidentStatus.InProgress, incident.StatusHistory.First().ToStatus);
    }

    [Fact]
    public void ChangeStatus_FromOpenToResolved_ThrowsDomainException()
    {
        var incident = Incident.Create("Cache eviction bug", "keys expiring early", IncidentSeverity.Sev3, Guid.NewGuid());

        Assert.Throws<DomainException>(() => incident.ChangeStatus(IncidentStatus.Resolved, Guid.NewGuid()));
    }

    [Fact]
    public void ChangeStatus_FromClosed_ThrowsDomainException()
    {
        var incident = Incident.Create("Auth token bug", "expiry off by one", IncidentSeverity.Sev2, Guid.NewGuid());
        var actorId = Guid.NewGuid();
        incident.ChangeStatus(IncidentStatus.InProgress, actorId);
        incident.ChangeStatus(IncidentStatus.Resolved, actorId);
        incident.ChangeStatus(IncidentStatus.Closed, actorId);

        Assert.Throws<DomainException>(() => incident.ChangeStatus(IncidentStatus.InProgress, actorId));
    }

    [Fact]
    public void ChangeStatus_ToSameStatus_IsNoOp()
    {
        var incident = Incident.Create("Slow queries", "p95 spiked", IncidentSeverity.Sev2, Guid.NewGuid());

        incident.ChangeStatus(IncidentStatus.Open, Guid.NewGuid());

        Assert.Equal(IncidentStatus.Open, incident.Status);
        Assert.Empty(incident.StatusHistory);
    }

    [Fact]
    public void AssignTo_MovesOpenIncidentToInProgress()
    {
        var incident = Incident.Create("Disk usage alert", "node-3 at 92%", IncidentSeverity.Sev2, Guid.NewGuid());
        var assigneeId = Guid.NewGuid();

        incident.AssignTo(assigneeId, Guid.NewGuid());

        Assert.Equal(assigneeId, incident.AssignedToId);
        Assert.Equal(IncidentStatus.InProgress, incident.Status);
    }

    [Fact]
    public void AssignTo_ClosedIncident_ThrowsDomainException()
    {
        var incident = Incident.Create("Expired cert", "TLS handshake failing", IncidentSeverity.Sev1, Guid.NewGuid());
        var actorId = Guid.NewGuid();
        incident.ChangeStatus(IncidentStatus.InProgress, actorId);
        incident.ChangeStatus(IncidentStatus.Resolved, actorId);
        incident.ChangeStatus(IncidentStatus.Closed, actorId);

        Assert.Throws<DomainException>(() => incident.AssignTo(Guid.NewGuid(), actorId));
    }

    [Fact]
    public void IsSlaBreached_ReturnsFalse_ForResolvedIncident()
    {
        var incident = Incident.Create("Minor UI glitch", "button misaligned", IncidentSeverity.Sev3, Guid.NewGuid());
        var actorId = Guid.NewGuid();
        incident.ChangeStatus(IncidentStatus.InProgress, actorId);
        incident.ChangeStatus(IncidentStatus.Resolved, actorId);

        Assert.False(incident.IsSlaBreached);
        Assert.NotNull(incident.ResolvedAt);
    }
}
