using IncidentManagement.Domain.Common;
using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class IncidentStatusChange : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Incident? Incident { get; set; }

    public IncidentStatus FromStatus { get; set; }
    public IncidentStatus ToStatus { get; set; }

    public Guid ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }
}
