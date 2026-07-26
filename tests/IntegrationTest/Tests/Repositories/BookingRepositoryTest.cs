using EventTests.Tests.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using yandex_pract.DbContext;

namespace EventTests.Tests.Repositories;

[Collection("Database")]
public sealed class BookingRepositoryTest : BaseTestRepository
{
	protected override AppDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(_postgres.GetConnectionString())
			.Options;

		var context = new AppDbContext(options);
		context.Database.EnsureCreated();
		return context;
	}

	protected override async Task ResetDatabaseAsync()
	{
		await using var context = CreateContext();
		await context.Database.ExecuteSqlRawAsync(
			"TRUNCATE TABLE books, authors RESTART IDENTITY CASCADE");
	}
}