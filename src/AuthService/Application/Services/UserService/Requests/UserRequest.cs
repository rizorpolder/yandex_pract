namespace Application.Services.UserService.Requests;

public record UserRequest
{
	public string Login { get; set; }
	public string Password { get; set; }
}