using System.Reflection;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
	public DbSet<User> Users => Set<User>();

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(Assembly
			.GetExecutingAssembly()); // получить все настройки из Configurations текущей сборки
		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}