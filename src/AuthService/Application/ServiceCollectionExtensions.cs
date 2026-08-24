using Application.Services.Abstraction.Services;
using Application.Services.UserService;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		services.AddScoped<IUserService, UserService>();
		return services;
	}
}