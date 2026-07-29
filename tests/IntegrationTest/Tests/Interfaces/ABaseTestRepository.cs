using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using yandex_pract.DbContext;
using yandex_pract.Interceptors;

namespace IntegrationTest.Tests.Interfaces;

public abstract class ABaseTestRepository : IAsyncLifetime
{
	protected readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
		.Build();
		

	public async Task InitializeAsync() => await _postgres.StartAsync();
	public async Task DisposeAsync() => await _postgres.DisposeAsync();
	protected virtual string[] TablesToTruncate => ["events", "bookings"];

	protected AppDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(_postgres.GetConnectionString())
			.AddInterceptors(new DateTimeInterceptor())
			.Options;

		return  new AppDbContext(options);
	}

	protected async Task ResetDatabaseAsync()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var tables = string.Join(", ", TablesToTruncate);
		var sql = $"TRUNCATE TABLE {tables} RESTART IDENTITY CASCADE";

		await ctx.Database.ExecuteSqlRawAsync(sql);
	}
}