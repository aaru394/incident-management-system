using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Infrastructure.Persistence.Repositories;

public class IncidentRepository(AppDbContext context) : IIncidentRepository
{
    public Task<Incident?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Incidents.FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<Incident?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        context.Incidents
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .Include(i => i.Comments).ThenInclude(c => c.User)
            .Include(i => i.StatusHistory)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<(IReadOnlyList<Incident> Items, int TotalCount)> QueryAsync(IncidentQueryParameters parameters, CancellationToken ct = default)
    {
        var query = context.Incidents
            .Include(i => i.Reporter)
            .Include(i => i.AssignedTo)
            .AsQueryable();

        if (parameters.Status.HasValue)
        {
            query = query.Where(i => i.Status == parameters.Status.Value);
        }

        if (parameters.Severity.HasValue)
        {
            query = query.Where(i => i.Severity == parameters.Severity.Value);
        }

        if (parameters.AssignedToId.HasValue)
        {
            query = query.Where(i => i.AssignedToId == parameters.AssignedToId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task AddAsync(Incident incident, CancellationToken ct = default)
    {
        context.Incidents.Add(incident);
        return Task.CompletedTask;
    }

    public void Update(Incident incident) => context.Incidents.Update(incident);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
