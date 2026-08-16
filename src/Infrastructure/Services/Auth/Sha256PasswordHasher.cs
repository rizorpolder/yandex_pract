using System.Security.Cryptography;
using System.Text;
using Application.Services.Abstraction.Services.Auth;

namespace Infrastructure.Services.Auth;

public class Sha256PasswordHasher : IPasswordHasher
{
	public string GetHash(string password)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToHexString(bytes); 
	}

	public bool VerifyHash(string password, string hash)
	{
		return GetHash(password) == hash;
	}
}