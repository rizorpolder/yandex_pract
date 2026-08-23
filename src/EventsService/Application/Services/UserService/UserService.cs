using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.RequestResult;
using Application.Services.Abstraction.Services;
using Application.Services.Abstraction.Services.Auth;
using Domain.Models.Users;

namespace Application.Services.UserService;

public class UserService(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtGenerator jwtGenerator) : IUserService
{
	public async Task<Result<string>> RegisterAsync(string login, string password, UserRole role = UserRole.User)
	{
		var exist = await userRepository.GetByLoginAsync(login);
		if (exist != null)
			return Result<string>.Failure("User already exists");


		var hash = passwordHasher.GetHash(password);
		var user = new User(login, hash, role);

		await userRepository.AddAsync(user);
		await userRepository.SaveChangesAsync();
		var token = jwtGenerator.GenerateJwtToken(user.Id, login, user.Role);
		return Result<string>.Success(token);
	}

	public async Task<Result<string>> LoginAsync(string login, string password)
	{
		var user = await userRepository.GetByLoginAsync(login);
		if (user == null || !passwordHasher.VerifyHash(password, user.PasswordHash))
			return Result<string>.Failure("Invalid login or password");

		var token = jwtGenerator.GenerateJwtToken(user.Id, user.Login, user.Role);
		return Result<string>.Success(token);
	}
}