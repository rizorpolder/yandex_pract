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
			var bookingResult = await bookingService.CreateBookingAsync(eventId);

			if (!bookingResult.IsSuccess)
				return NotFound(new {Message = bookingResult.ErrorMessage});

			var location = $"/events/bookings/{bookingResult.Value.Id}";
			Response.Headers.Append("Location", location);

			return Accepted(bookingResult.Value);
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
		var bookingResult = await bookingService.GetBookingByIdAsync(bookingId);

		if (!bookingResult.IsSuccess)
			return NotFound();

		return Ok(bookingResult.Value);
	}
}