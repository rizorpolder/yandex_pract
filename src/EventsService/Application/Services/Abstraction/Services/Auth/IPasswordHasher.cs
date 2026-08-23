namespace Application.Services.Abstraction.Services.Auth;

public interface IPasswordHasher
{
	public string GetHash(string password);
	public bool VerifyHash(string password, string hash);
}