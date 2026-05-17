using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yandex_pract.MockDB;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.Services.BookingService;

[ApiController]
[Route("events")]
public class BookingController : ControllerBase
{
	private readonly IBookingService bookingService;

	public BookingController(IBookingService bookingService)
	{
		this.bookingService = bookingService;
	}

	[HttpPost("{eventId:guid}/book")]
	public async Task<IActionResult> AddBooking(Guid eventId)
	{
		var (result, booking) = await bookingService.CreateBookingAsync(eventId);

		if (!result)
			return NotFound(new { Message = "Event not found" });

		var location = $"/events/bookings/{booking.Id}";
		Response.Headers.Append("Location", location);

		return Accepted(new
		{
			booking.Id,
			booking.EventId,
			booking.Status
		});
	}

	[HttpGet("bookings/{bookingId:guid}")]
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