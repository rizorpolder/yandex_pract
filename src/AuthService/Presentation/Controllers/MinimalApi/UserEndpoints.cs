using Application.Services.Abstraction.Services;
using Application.Services.UserService.Requests;
using Domain.Models.Users;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Presentation.Controllers;

internal static class UserEndpoints
{
	public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
	{
		var group = builder.MapGroup("/auth");
		group.MapPost("/register",
				async (IUserService userService,
					[FromBody] UserRequest userRequest) =>
				{
					var result = await userService.RegisterAsync(userRequest.Login, userRequest.Password);
					return !result.IsSuccess
						? Results.BadRequest(new {Message = result.ErrorMessage})
						: Results.Ok(result);
				})
			.WithName("Register")
			.Produces<ProblemDetails>(StatusCodes.Status200OK)
			.Produces<ProblemDetails>(StatusCodes.Status400BadRequest);


		group.MapPost("/login",
				async (IUserService userService,
					[FromBody] UserRequest userRequest) =>
				{
					var result = await userService.LoginAsync(userRequest.Login, userRequest.Password);
					return !result.IsSuccess ? Results.Unauthorized() : Results.Ok(result.Value);
				})
			.WithName("Login")
			.Produces<ProblemDetails>(StatusCodes.Status200OK)
			.Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

		group.MapPost("/admin/create",
				async (IUserService userService,
					[FromBody] UserRequest userRequest) =>
				{
					var result =
						await userService.RegisterAsync(userRequest.Login, userRequest.Password, UserRole.Admin);
					if (!result.IsSuccess)
						return Results.BadRequest(new {Message = result.ErrorMessage});

					return Results.Ok();
				})
			.Produces<ProblemDetails>(StatusCodes.Status200OK)
			.Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
			.RequireAuthorization(policyBuilder =>
				policyBuilder.AddRequirements(new RolesAuthorizationRequirement([
					"Admin"
				])));

		return group;
	}
}