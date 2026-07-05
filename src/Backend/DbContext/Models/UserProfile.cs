namespace yandex_pract.DbContext;

public class UserProfile
{
	public int Id { get; set; }
	public string Bio { get; set; }
	public string Address { get; set; }
	public int UserId { get; set; }
	public User User { get; set; }
}