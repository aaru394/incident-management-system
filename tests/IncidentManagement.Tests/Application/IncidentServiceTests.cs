using IncidentManagement.Application.Common;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Interfaces;
using IncidentManagement.Application.Services;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IncidentManagement.Tests.Application;

public class IncidentServiceTests
{
    private readonly Mock<IIncidentRepository> _incidentRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly IncidentService _sut;

    public IncidentServiceTests()
    {
        _sut = new IncidentService(_incidentRepository.Object, _userRepository.Object, _cache.Object, NullLogger<IncidentService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFound_WhenReporterMissing()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new CreateIncidentRequest("Title", "Description", IncidentSeverity.Sev1);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(request, Guid.NewGuid(), default));
    }

    [Fact]
    public async Task CreateAsync_PersistsIncident_AndReturnsMappedResponse()
    {
        var reporter = new User { FullName = "Aryan Koul", Email = "aryan@example.com" };
        _userRepository.Setup(r => r.GetByIdAsync(reporter.Id, It.IsAny<CancellationToken>())).ReturnsAsync(reporter);

        var request = new CreateIncidentRequest("API down", "500s on checkout", IncidentSeverity.Sev1);

        var result = await _sut.CreateAsync(request, reporter.Id, default);

        _incidentRepository.Verify(r => r.AddAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()), Times.Once);
        _incidentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("API down", result.Title);
        Assert.Equal(reporter.FullName, result.ReporterName);
        Assert.Equal(IncidentStatus.Open, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCachedValue_WithoutHittingRepository()
    {
        var cached = new IncidentResponse(
            Guid.NewGuid(), "Cached title", "desc", IncidentSeverity.Sev2, IncidentStatus.Open,
            DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(24), false, null,
            Guid.NewGuid(), "Reporter", null, null,
            [], []);

        _cache.Setup(c => c.GetAsync<IncidentResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _sut.GetByIdAsync(cached.Id, default);

        Assert.Equal(cached, result);
        _incidentRepository.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeStatusAsync_ThrowsNotFound_WhenIncidentMissing()
    {
        _incidentRepository.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ChangeStatusAsync(Guid.NewGuid(), new UpdateIncidentStatusRequest(IncidentStatus.InProgress), Guid.NewGuid(), default));
    }
}
