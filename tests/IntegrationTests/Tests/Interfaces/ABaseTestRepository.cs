using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;
using yandex_pract.DbContext;
using yandex_pract.Interceptors;

namespace IntegrationTest.Tests.Interfaces;

public abstract class ABaseTestRepository(PostgresContainerFixture fixture)
{
	protected readonly PostgresContainerFixture _fixture = fixture;

	protected virtual string[] TablesToTruncate => ["events", "bookings"];

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