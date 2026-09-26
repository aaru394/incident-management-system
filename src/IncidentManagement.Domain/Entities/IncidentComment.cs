using IncidentManagement.Domain.Common;

namespace IncidentManagement.Domain.Entities;

public class IncidentComment : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Incident? Incident { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Message { get; set; } = string.Empty;
}
