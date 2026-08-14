using System.Text;
using Application.Services.Abstraction.Repositories;
using Infrastructure.Contexts;
using Infrastructure.Interceptors;
using Infrastructure.Repositories;
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

		services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = "SecureApi";
				options.DefaultChallengeScheme = "SecureApi";
			})
			.AddJwtBearer("SecureApi",
				options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						RoleClaimType = "role",

						ValidateIssuer = true,
						ValidIssuer = "MyAuthServer",

						ValidateAudience = true,
						ValidAudience = "MyClientApp",

						ValidateLifetime = true,
						ClockSkew = TimeSpan.FromMinutes(3),

						ValidateIssuerSigningKey = true,
						IssuerSigningKey =
							new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperLongSecretKey12345678901234567")),
					};
				});

		services.AddAuthorization(options =>
		{
			options.AddPolicy("AdultAdmin",
				policy => policy.RequireRole("Admin")
					.RequireAssertion(ctx => ctx.User
						.HasClaim(c => c.Type == "Age" && int.Parse(c.Value) >= 18))); //политика роли админ >18 лет
		});

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