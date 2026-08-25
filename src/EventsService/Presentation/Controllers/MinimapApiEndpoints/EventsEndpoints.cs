using EventsService.Application.Services.Abstraction.Services;
using EventsService.Application.Services.EventService.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProblemDetails = Presentation.Middleware.ProblemDetails;

namespace EventsService.Presentation.Controllers.MinimapApiEndpoints;

internal static class EventsEndpoints
{
	public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder builder)
	{
		var group = builder.MapGroup("/events");

		MapGetEventsEndpoint(group);
		MapGetEventByIdEndpoint(group);
		MapCreateEventEndpoint(group);
		MapUpdateEventByIdEndpoint(group);
		MapDeleteEventByIdEndpoint(group);
		MapGetTopEventsEndpoint(group);
		return group;
	}
	
	private static void MapGetEventsEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/",
				async (
					IEventService eventService,
					string? title,
					DateTime? from,
					DateTime? to,
					int page = 1,
					int pageSize = 10) =>
				{
					var result = await eventService.GetEvents(title, from, to, page, pageSize);
					return Results.Ok(result);
				})
			.WithName("GetEvents")
			.Produces<PaginatedResultDto>()
			.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
	}

	private static void MapGetEventByIdEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("{id:guid}",
				async (
					IEventService eventService,
					Guid eventId) =>
				{
					var result = await eventService.GetEventById(eventId);

					if (!result.IsSuccess)
						return Results.NotFound(new {Message = result.ErrorMessage});

					return Results.Ok(result.Value);
				})
			.Produces<EventDto>()
			.Produces<ProblemDetails>(StatusCodes.Status404NotFound);
	}

	private static void MapCreateEventEndpoint(RouteGroupBuilder group)
	{
		group.MapPost("/",
				async (
					IEventService eventService,
					[FromBody] EventDto eventDto,
					HttpContext ctx) =>
				{
					var validator = ctx.RequestServices.GetRequiredService<IObjectModelValidator>();

					var actionContext = new ActionContext(
						ctx,
						ctx.GetRouteData(),
						new ActionDescriptor(),
						new ModelStateDictionary()
					);

					validator.Validate(actionContext, null, string.Empty, eventDto);

					if (!actionContext.ModelState.IsValid)
						return Results.BadRequest();

					var result = await eventService.CreateEventAsync(eventDto);
					if (!result.IsSuccess)
						return Results.BadRequest(new {Message = result.ErrorMessage});

					return Results.Ok(result.Value);
				}).WithName("CreateEvent")
			.RequireAuthorization(policy => policy.RequireRole("Admin"))
			.Produces<EventDto>(StatusCodes.Status201Created)
			.Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
	}

	private static void MapUpdateEventByIdEndpoint(RouteGroupBuilder group)
	{
		group.MapPut("{id:guid}",
				async (
					IEventService eventService,
					Guid id,
					[FromBody] EventDto eventDto,
					HttpContext ctx
				) =>
				{
					var validator = ctx.RequestServices.GetRequiredService<IObjectModelValidator>();

					var actionContext = new ActionContext(
						ctx,
						ctx.GetRouteData(),
						new ActionDescriptor(),
						new ModelStateDictionary()
					);

					validator.Validate(actionContext, null, string.Empty, eventDto);

					if (!actionContext.ModelState.IsValid)
						return Results.BadRequest();

					var result = await eventService.UpdateEventAsync(id, eventDto);
					if (!result.IsSuccess)
						return Results.NotFound(new {Message = result.ErrorMessage});
					return Results.Ok(result.Value);
				})
			.WithName("UpdateEventById")
			.RequireAuthorization(policy => policy.RequireRole("Admin"))
			.Produces<EventDto>()
			.Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
			.Produces<ProblemDetails>(StatusCodes.Status404NotFound);
	}

	private static void MapDeleteEventByIdEndpoint(RouteGroupBuilder group)
	{
		group.MapDelete("{id:guid}",
				async (IEventService service, Guid id) =>
				{
					var getEvtResult = await service.GetEventById(id);
					if (!getEvtResult.IsSuccess)
						return Results.NotFound(new {message = getEvtResult.ErrorMessage});

					var removeEvtResult = await service.RemoveEvent(getEvtResult.Value);
					if (!removeEvtResult.IsSuccess)
						return Results.BadRequest(new {message = removeEvtResult.ErrorMessage});

					return Results.Ok();
				})
			.WithName("DeleteEventById")
			.RequireAuthorization(policy => policy.RequireRole("Admin"))
			.Produces(StatusCodes.Status200OK)
			.Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
	}
	
	private static void MapGetTopEventsEndpoint(RouteGroupBuilder group)
	{
		group.MapGet("/top", async (IEventService eventService) =>
			{
				var result = await eventService.GetTopEventsAsync();
				return Results.Ok(result);
			})
			.WithName("GetTopEvents")
			.Produces<List<EventDto>>();
	}
}