using Application.Services.Abstraction.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Presentation.Middleware;
using yandex_pract.CustomEventService.Dto;

namespace Presentation.Controllers;

internal static class EventsEndpoints
{
	//Другой способ определения эндпойнтов
	public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder builder)
	{
		// var group = builder.MapGroup("/events");
		// group.MapGet("/", async (
		// 		IEventService eventService,
		// 		string? title,
		// 		DateTime? from,
		// 		DateTime? to,
		// 		int page = 1,
		// 		int pageSize = 10) =>
		// 	{
		// 		var result = await eventService.GetEvents(title, from, to, page, pageSize);
		// 		return Results.Ok(result);
		// 	})
		// 	.WithName("GetEvents")
		// 	.Produces<PaginatedResultDto>()
		// 	.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
		//
		// group.MapGet("/{id:guid}", async (IEventService eventService, Guid id) =>
		// 	{
		// 		var result = await eventService.GetEventById(id);
		// 		if (result.hasElement)
		// 			return Results.Ok(new EventDto(result.resultModel));
		// 		return Results.NotFound();
		// 	})
		// 	.WithName("GetEventById")
		// 	.Produces<EventDto>()
		// 	.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
		// 	.Produces<ProblemDetails>(StatusCodes.Status404NotFound);
		//
		// group.MapDelete("/{id:guid}", async (IEventService eventService, Guid id) =>
		// 	{
		// 		var model = await eventService.GetEventById(id);
		// 		if (!model.hasElement)
		// 			return Results.NotFound();
		//
		// 		var isSuccess = await eventService.RemoveEvent(model.resultModel);
		// 		if (isSuccess)
		// 			return Results.Ok();
		//
		// 		return Results.BadRequest();
		// 	})
		// 	.WithName("DeleteEventById")
		// 	.Produces(StatusCodes.Status204NoContent)
		// 	.Produces(StatusCodes.Status500InternalServerError);
		return builder;
	}
}