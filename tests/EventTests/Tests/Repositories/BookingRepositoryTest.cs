using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using yandex_pract.DbContext;

namespace EventTests.Tests.Repositories;

public class BookingRepositoryTest: IAsyncLifetime
{
	private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
		.WithImage("postgres:16-alpine")
		.Build();

	public async Task InitializeAsync()
	{
		await _postgres.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await _postgres.DisposeAsync();
	}
	
	private AppDbContext CreateContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql(_postgres.GetConnectionString())
			.Options;

		var context = new AppDbContext(options);
		context.Database.EnsureCreated();
		return context;
	} 
} 