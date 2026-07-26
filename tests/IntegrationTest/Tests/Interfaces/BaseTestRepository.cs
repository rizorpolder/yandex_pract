using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using yandex_pract.DbContext;

namespace IntegrationTest.Tests.Interfaces;

public abstract class BaseTestRepository : IAsyncLifetime
{
	protected readonly PostgreSqlContainer _postgres =
		new PostgreSqlBuilder()
			.WithImage("postgres:16-alpine")
			.Build();

	public async Task InitializeAsync()
	{
		await _postgres.StartAsync();
		await using var ctx = CreateContextInternal();
		await ctx.Database.MigrateAsync();
	}

	public async Task DisposeAsync() => await _postgres.DisposeAsync();

	protected virtual string[] TablesToTruncate => new[] { "events", "bookings" };

	protected virtual AppDbContext CreateContext()
	{
		return CreateContextInternal();
	}

	private AppDbContext CreateContextInternal()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(_postgres.GetConnectionString())
			.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
			.Options;

		return new AppDbContext(options);
	}

	protected async Task ResetDatabaseAsync()
	{
		await using var ctx = CreateContextInternal();

		var tables = string.Join(", ", TablesToTruncate);
		var sql = $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE";

		await ctx.Database.ExecuteSqlRawAsync(sql);
	}
}