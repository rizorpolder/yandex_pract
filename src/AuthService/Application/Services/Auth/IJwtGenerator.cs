using Common.Models;
using Domain.Models.Users;

namespace Application.Services.Abstraction.Services.Auth;

public interface IJwtGenerator
{
	public string GenerateJwtToken(Guid userId, string login, UserRole role);
}