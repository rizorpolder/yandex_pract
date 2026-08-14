using Application;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.Middleware;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPresentation(this IServiceCollection services,IConfiguration configuration)
	{
		services.AddControllers();

		services.AddSwaggerGen(options =>
		{
			options.CustomOperationIds(apiDesc =>
				apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);
		});

		services.AddInfrastructure(configuration);
		services.AddApplication();

		return services;
	}

	public static IApplicationBuilder UsePresentation(this IApplicationBuilder app)
	{
		app.UseInfrastructure();

		app.UseMiddleware<ErrorCustomMiddleware>();

		if (app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseRouting();
		return app;
	}

	public static IEndpointRouteBuilder MapPresentationEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapControllers();
		return endpoints;
	}
}