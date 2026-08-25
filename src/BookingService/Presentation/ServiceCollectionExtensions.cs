using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Presentation;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPresentation(this IServiceCollection services)
	{
		services.AddControllers()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});
		services.AddSwaggerGen(options =>
		{
			options.AddSecurityDefinition("Bearer",
				new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "Введите JWT токен",
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT"
				});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});
		});
		return services;
	}

	public static IApplicationBuilder UsePresentation(this IApplicationBuilder app)
	{
		var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

		if (env.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				options.RoutePrefix = string.Empty; // UI теперь на "/"
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
			});
		}

		return app;
	}

	public static IEndpointRouteBuilder MapPresentationEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapControllers();
		return endpoints;
	}
}