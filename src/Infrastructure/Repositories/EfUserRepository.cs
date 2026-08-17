using Application.Services.Abstraction.Repositories;
using Domain.Models.Users;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EfUserRepository(AppDbContext dbContext) : IUserRepository
{
	public async Task<User?> GetByLoginAsync(string login)
	{
		return await dbContext.Users.FirstOrDefaultAsync(x => x.Login == login);
	}

	public Task AddAsync(User user)
	{
		dbContext.Users.Add(user);
		return Task.CompletedTask;
	}

	public async Task SaveChangesAsync()
	{
		await dbContext.SaveChangesAsync();
	}
}