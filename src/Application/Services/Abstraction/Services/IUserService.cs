using Application.Services.Abstraction.RequestResult;

namespace Application.Services.Abstraction.Services;


public interface IUserService
{
	Task<Result<string>> RegisterAsync(string login, string password);
	Task<Result<string>> LoginAsync(string login, string password);
}