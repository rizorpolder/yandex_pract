using System;
using Microsoft.EntityFrameworkCore;

namespace yandex_pract.DbContext;

public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<Product> Products { get; set; }
	public DbSet<PriceHistory> PriceHistory { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseInMemoryDatabase("MyTestDatabase");
	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<Product>()
			.Property(p => p.Attributes)
			.HasColumnType("jsonb");  // фильт по словарю 
		// Поиск товаров, у которых в JSONB-колонке указан конкретный бренд
		// var products = await db.Products
		// 	.Where(p => p.Attributes["Brand"] == "Samsung")
		// 	.ToListAsync(); 
		
		
		// Конфигурация во Fluent API (в методе OnModelCreating)
		builder.Entity<User>().OwnsOne(u => u.Profile, builder => {
			builder.ToJson(); // EF Core 7+ пометит колонку как jsonb
		});
		
		builder.Entity<PriceHistory>()
			.HasOne<Product>()
			.WithMany(p => p.PriceHistory)
			.HasForeignKey(p => p.ProductId);
		
		builder.Entity<UserProfile>().HasData(
			new UserProfile { Id = 1, Bio = "Senior Developer", Address = "Moscow" },
			new UserProfile { Id = 2, Bio = "UI/UX Designer", Address = "Berlin" },
			new UserProfile { Id = 3, Bio = "QA Engineer", Address = "Prague" }
		);

		builder.Entity<Product>().HasData(
			new Product { Id = 1, Name = "Laptop Pro" },
			new Product { Id = 2, Name = "Gaming Mouse" }
		);

		builder.Entity<PriceHistory>().HasData(
			new PriceHistory { Id = 1, ProductId = 1, Date = new DateTime(2023, 1, 1), Price = 1200 },
			new PriceHistory { Id = 2, ProductId = 1, Date = new DateTime(2023, 5, 1), Price = 1500 },
			new PriceHistory { Id = 3, ProductId = 1, Date = new DateTime(2023, 12, 1), Price = 1300 },
			new PriceHistory { Id = 4, ProductId = 2, Date = new DateTime(2023, 1, 1), Price = 50 },
			new PriceHistory { Id = 5, ProductId = 2, Date = new DateTime(2023, 11, 1), Price = 45 }
		);
		
	}
}