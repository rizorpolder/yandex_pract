using Application.Services.Abstraction.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("events")]
public class BookingController : ControllerBase
{
	private readonly IBookingService bookingService;

	public BookingController(IBookingService bookingService)
	{
		this.bookingService = bookingService;
	}

	[HttpPost("{eventId:guid}/book", Name = nameof(AddBooking))]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	public async Task<IActionResult> AddBooking(Guid eventId)
	{
		try
		{
			var (result, booking) = await bookingService.CreateBookingAsync(eventId);

			if (!result)
				return NotFound(new {Message = "Event not found"});

			var location = $"/events/bookings/{booking.Id}";
			Response.Headers.Append("Location", location);

			return Accepted(new
			{
				booking.Id,
				booking.EventId,
				booking.Status
			});
		}
		catch (NoAvailableSeatsException)
		{
			return Conflict(new {Message = "No available seats"});
		}
	}

	[HttpGet("bookings/{bookingId:guid}", Name = nameof(GetBooking))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> GetBooking(Guid bookingId)
	{
		var (haveBooking, booking) = await bookingService.GetBookingByIdAsync(bookingId);

		if (!haveBooking)
			return NotFound();

		return Ok(new
		{
			booking.Id,
			booking.EventId,
			booking.Status
		});
	}
}