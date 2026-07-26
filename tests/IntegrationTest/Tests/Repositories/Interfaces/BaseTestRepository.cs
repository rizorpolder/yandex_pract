using Testcontainers.PostgreSql;
using yandex_pract.DbContext;

namespace EventTests.Tests.Repositories.Interfaces;

public abstract class BaseTestRepository : IAsyncLifetime
{
	protected readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
		.WithImage("postgres:16-alpine")
		.Build();

	public async Task InitializeAsync() => await _postgres.StartAsync();
	public async Task DisposeAsync() => await _postgres.DisposeAsync();

	protected abstract AppDbContext CreateContext();
	protected abstract Task ResetDatabaseAsync();

}