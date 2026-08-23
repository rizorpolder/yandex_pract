using Infrastructure.Contexts;
using Infrastructure.Interceptors;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTest.Tests.Interfaces;

public abstract class ABaseTestRepository(PostgresContainerFixture fixture)
{
	protected readonly PostgresContainerFixture _fixture = fixture;

	protected virtual string[] TablesToTruncate => ["events", "bookings", "users"];

	protected AppDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(_fixture.Postgres.GetConnectionString())
			.AddInterceptors(new DateTimeInterceptor())
			.Options;

		return new AppDbContext(options);
	}

	protected async Task ResetDatabaseAsync()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var tables = string.Join(", ", TablesToTruncate);
		await ctx.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE");
	}
}