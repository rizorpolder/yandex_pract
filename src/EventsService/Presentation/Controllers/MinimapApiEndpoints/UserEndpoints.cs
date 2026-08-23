using Application.Services.Abstraction.Services;
using Domain.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Presentation.Controllers;

internal static class UserEndpoints
{
	public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
	{
		var group = builder.MapGroup("/auth");
		group.MapPost("/register", async (IUserService userService,
				string login,
				string password,
				UserRole role = UserRole.User) =>
			{
				var result = await userService.RegisterAsync(login, password);
				return Results.Ok(result);
			})
			.WithName("Register");

		group.MapPost("/login",
				async (IUserService userService,
					string login,
					string password) =>
				{
					var result = await userService.LoginAsync(login, password);
					if (result.IsSuccess)
						return Results.Ok(result);

					return Results.BadRequest();
				})
			.WithName("Login");
		return group;
	}
}