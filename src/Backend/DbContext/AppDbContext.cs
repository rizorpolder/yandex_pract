using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace yandex_pract.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
	public DbSet<Book> Books { get; set; }
	public DbSet<Book> Authors { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfigurationsFromAssembly(Assembly
			.GetExecutingAssembly()); // получить все настройки из Configurations текущей сборки
	}
}