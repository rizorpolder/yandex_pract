namespace Domain.Models.Users;

public class User
{
	public Guid UserId { get; set; }
	public UserRole Role { get; set; }
	public string PasswordHash { get; set; }
}