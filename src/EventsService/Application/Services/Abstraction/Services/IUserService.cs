using Application.Services.Abstraction.RequestResult;
using Domain.Models.Users;

namespace Application.Services.Abstraction.Services;


public interface IUserService
{
	Task<Result<string>> RegisterAsync(string login, string password, UserRole role =  UserRole.User);
	Task<Result<string>> LoginAsync(string login, string password);
}