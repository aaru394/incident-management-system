using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
