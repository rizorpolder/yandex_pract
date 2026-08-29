using System;
using System.Text;
using Common.Extensions;
using EventsService.Application;
using EventsService.Infrastructure;
using EventsService.Presentation;
using EventsService.Presentation.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddPresentation();
		builder.Services.AddInfrastructure(builder.Configuration);
		builder.Services.AddApplication();
		builder.Services.AddObservability(builder.Configuration, serviceName:"EventsService");
		
		builder.UseSerilog();
		
		AddAuth(builder);
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
		app.Run();
	}

	private static void AddAuth(WebApplicationBuilder builder)
	{
		var jwtSection = builder.Configuration.GetSection("Jwt");

		var issuer = jwtSection["Issuer"]
		             ?? throw new InvalidOperationException("Не задан Jwt:Issuer в конфигурации.");
		var audience = jwtSection["Audience"]
		               ?? throw new InvalidOperationException("Не задан Jwt:Audience в конфигурации.");
		var signingKey = jwtSection["Secret"]
		                 ?? throw new InvalidOperationException("Не задан Jwt:SigningKey в конфигурации.");

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
						ValidIssuer = issuer,
						ValidateAudience = true,
						ValidAudience = audience,
						ValidateLifetime = true,
						ClockSkew = TimeSpan.FromMinutes(3),
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(signingKey))
					};
				});
	}
}