using Microsoft.EntityFrameworkCore;

namespace yandex_pract.DbContext;

public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
	public DbSet<User> Users { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseInMemoryDatabase("MyTestDatabase");
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<User>().HasData(
			new User { Id = 1, Name = "Alex" },
			new User { Id = 2, Name = "Bob" },
			new User { Id = 3, Name = "Sam" }
		);
	}
}

public class User
{
	public int Id { get; set; }
	public string Name { get; set; }
}