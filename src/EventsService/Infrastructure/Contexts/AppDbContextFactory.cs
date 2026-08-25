using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EventsService.Infrastructure.Contexts;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var connectionString =
			Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
			?? new ConfigurationBuilder()
				.AddUserSecrets<AppDbContextFactory>(optional: true)
				.AddEnvironmentVariables()
				.Build()
				.GetConnectionString("DefaultConnection")
			?? "Host=127.0.0.1;Port=5432;Database=events_db;Username=postgres;Password=postgres";

		var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
		optionsBuilder.UseNpgsql(connectionString);

		return new AppDbContext(optionsBuilder.Options);
	}
}