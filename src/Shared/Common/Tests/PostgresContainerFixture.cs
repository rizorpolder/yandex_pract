using Testcontainers.PostgreSql;
using Xunit;

namespace IntegrationTest.Tests.Fixture;

public class PostgresContainerFixture : IAsyncLifetime
{
	public PostgreSqlContainer Postgres { get; } =
		new PostgreSqlBuilder("postgres:16-alpine").Build();

	public async Task InitializeAsync() => await Postgres.StartAsync();
	public async Task DisposeAsync() => await Postgres.DisposeAsync();
}