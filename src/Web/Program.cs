using System;
using System.Text;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Presentation;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddPresentation(builder.Configuration);
		builder.Services.AddInfrastructure(builder.Configuration);
		builder.Services.AddAuthentication(options =>
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

		builder.Services.AddAuthorization(options =>
		{
			options.AddPolicy("AdultAdmin",
				policy => policy.RequireRole("Admin")
					.RequireAssertion(ctx => ctx.User
						.HasClaim(c => c.Type == "Age" && int.Parse(c.Value) >= 18))); //политика роли админ >18 лет
		});

		var app = builder.Build();

		app.UseInfrastructure();
		app.UsePresentation();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapPresentationEndpoints();

		app.Run();
	}
}