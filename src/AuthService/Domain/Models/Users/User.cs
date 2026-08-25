using Common.Models;

namespace Domain.Models.Users;

public class User
{
	public Guid Id { get; private set; }
	public string Login { get; private set; }
	public string PasswordHash { get; private set; }
	public UserRole Role { get; private set; }

	public User(string login, string passwordHash, UserRole role)
	{
		Id = Guid.NewGuid();
		Login = login;
		PasswordHash = passwordHash;
		Role = role;
	}
}