using Application.Services.Abstraction.Services;
using Application.Services.BookingService.Dto;
using Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Presentation.Middleware;

namespace Presentation.Controllers;

internal static class BookingEndpoints
{
	public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder builder)
	{
		builder.MapBookingCreate();
		builder.MapGetBooking();
		return builder;
	}


	private static IEndpointRouteBuilder MapBookingCreate(this IEndpointRouteBuilder builder)
	{
		builder.MapPost("{eventId:guid}/book}",
				async (IBookingService service, Guid eventId, HttpContext ctx) =>
				{
					try
					{
						var bookingResult = await service.CreateBookingAsync(eventId);

						if (!bookingResult.IsSuccess)
							return Results.NotFound(bookingResult.ErrorMessage);
						var location = $"/events/bookings/{bookingResult.Value.Id}";
						ctx.Response.Headers.Append("Location", location);
						return Results.Accepted(location, bookingResult.Value);
					}
					catch (NoAvailableSeatsException)
					{
						return Results.Conflict(new { Message = "No available seats" });
					}
				})
			.WithName("AddBooking")
			.Produces<BookingDto>(StatusCodes.Status202Accepted)
			.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
			.Produces<NoAvailableSeatsException>(StatusCodes.Status409Conflict)
			.Produces<ProblemDetails>(StatusCodes.Status200OK);

		return builder;
	}

	private static IEndpointRouteBuilder MapGetBooking(this IEndpointRouteBuilder builder)
	{
		builder.MapGet("bookings/{bookingId:guid}", async (IBookingService service, Guid bookingId) =>
			{
				var bookingResult = await service.GetBookingByIdAsync(bookingId);

				if (!bookingResult.IsSuccess)
					return Results.NotFound(bookingResult.ErrorMessage);

				return Results.Ok(bookingResult.Value);
			})
			.WithName("GetBooking")
			.Produces<BookingDto>()
			.Produces<ProblemDetails>();
		return builder;
	}
}