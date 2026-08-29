using System.Text;
using Application;
using Application.Services.Abstraction.Services.Auth;
using Common.Extensions;
using Common.Models;
using Domain.Models.Users;
using Infrastructure;
using Infrastructure.Contexts;
using Infrastructure.Services.Auth;
using Microsoft.IdentityModel.Tokens;
using Presentation;
using Presentation.Middleware;

namespace Web;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddPresentation();
		builder.Services.AddInfrastructure(builder.Configuration);
		builder.Services.AddApplication();
		builder.Services.AddObservability(builder.Configuration, serviceName: "AuthService");
		
		builder.UseSerilog();
		
		var jwtOptions = GetConfiguration<JwtOptions>(builder, "Jwt");

		builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = "Bearer";
				options.DefaultAuthenticateScheme = "Bearer";
				options.DefaultChallengeScheme = "Bearer";
			})
			.AddJwtBearer("Bearer",
				options =>
				{
					options.MapInboundClaims = false;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						RoleClaimType = "role",
						ValidateIssuer = true,
						ValidIssuer = jwtOptions.Issuer,
						ValidateAudience = true,
						ValidAudience = jwtOptions.Audience,
						ValidateLifetime = true,
						ClockSkew = TimeSpan.FromMinutes(3),
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(jwtOptions.Secret))
					};
				});

		builder.Services.AddAuthorization();

		var app = builder.Build();
		app.UseObservability();

		app.UseMiddleware<ErrorCustomMiddleware>();

		app.UseHttpsRedirection();
		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UsePresentation();
		app.UseInfrastructure();

		app.MapEndpoints();


		CreateSuperuser(app);

		app.Run();
	}

	private static void CreateSuperuser(WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
		var hasher = scope.ServiceProvider.GetService<IPasswordHasher>();
		if (!env.IsDevelopment())
			return;

		if (context.Users.Any(u => u.Role == UserRole.Admin))
			return;

		var admin = new User("admin", hasher.GetHash("dev-admin"), UserRole.Admin);
		context.Users.Add(admin);
		context.SaveChanges();
	}

	private static T? GetConfiguration<T>(WebApplicationBuilder builder, string key) where T : class
	{
		var section = builder.Configuration.GetSection(key);
		var option = section.Get<T>();
		if (option == null)
			return null;

		builder.Services.Configure<T>(section);
		return option;
	}
}