using Infrastructure.Interceptors;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Interfaces;

public abstract class ABaseTestRepository<T>(
	PostgresContainerFixture fixture,
	Func<DbContextOptions<T>, T> contextFactory)
	where T : DbContext
{
	protected readonly PostgresContainerFixture _fixture = fixture;
	protected virtual string[] TablesToTruncate => ["events", "bookings", "users"];

	protected T CreateContext()
	{
		var options = new DbContextOptionsBuilder<T>()
			.UseNpgsql(_fixture.Postgres.GetConnectionString())
			.AddInterceptors(new DateTimeInterceptor())
			.Options;

		return contextFactory(options);
	}

	protected async Task ResetDatabaseAsync()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var tables = string.Join(", ", TablesToTruncate);
		await ctx.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE");
	}
}