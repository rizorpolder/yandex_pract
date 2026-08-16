using System;
using System.Text;
using Application;
using Infrastructure;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Presentation;
using Presentation.Middleware;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddPresentation(builder.Configuration);
		builder.Services.AddInfrastructure(builder.Configuration);
		builder.Services.AddApplication();

		var jwtSection = builder.Configuration.GetSection("Jwt");
		var jwtOptions = jwtSection.Get<JwtOptions>();
		builder.Services.Configure<JwtOptions>(jwtSection);

		builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = "Bearer";
				options.DefaultAuthenticateScheme = "Bearer";
				options.DefaultChallengeScheme = "Bearer";
			})
			.AddJwtBearer("Bearer", options =>
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

		app.UseMiddleware<ErrorCustomMiddleware>();

		app.UseHttpsRedirection();
		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UsePresentation();
		app.MapPresentationEndpoints();

		app.Run();
	}
}