using Domain.Models.Users;

namespace Application.Services.Abstraction.Repositories;

public interface IUserRepository
{
	Task<User?> GetByLoginAsync(string login);
	Task AddAsync(User user);
	Task SaveChangesAsync();
}