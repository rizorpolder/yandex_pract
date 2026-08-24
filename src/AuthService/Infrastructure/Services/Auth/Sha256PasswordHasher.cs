using Application.Services.Abstraction.Services.Auth;
using Domain.Models.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Auth;

public class Sha256PasswordHasher : IPasswordHasher
{
	private readonly PasswordHasher<User> _hasher = new();

	public string GetHash(string password)
	{
		return _hasher.HashPassword(null!, password);
	}

	public bool VerifyHash(string password, string hash)
	{
		var result = _hasher.VerifyHashedPassword(null!, hash, password);
		return result == PasswordVerificationResult.Success;
	}
}