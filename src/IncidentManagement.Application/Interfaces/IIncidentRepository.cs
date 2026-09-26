using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Application.Interfaces;

public interface IIncidentRepository
{
    Task<Incident?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Incident?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Incident> Items, int TotalCount)> QueryAsync(IncidentQueryParameters parameters, CancellationToken ct = default);
    Task AddAsync(Incident incident, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
