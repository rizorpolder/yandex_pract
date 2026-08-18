using System.Text;
using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services.Auth;
using Infrastructure.Contexts;
using Infrastructure.Interceptors;
using Infrastructure.Repositories;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");

		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseNpgsql(connectionString);
			options.AddInterceptors(new DateTimeInterceptor());
		});

		services.AddScoped<IEventRepository, EfEventRepository>();
		services.AddScoped<IBookingRepository, EfBookingRepository>();
		services.AddScoped<IUserRepository, EfUserRepository>();
		
		services.AddScoped<IJwtGenerator, JwtGenerator>();
		services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();

		return services;
	}

	public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
	{
		using var scope = app.ApplicationServices.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		db.Database.Migrate();

		return app;
	}
}