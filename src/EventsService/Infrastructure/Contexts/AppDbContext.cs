using System.Reflection;
using Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class AppDbContext : DbContext
{
	public DbSet<Event> Events => Set<Event>();

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