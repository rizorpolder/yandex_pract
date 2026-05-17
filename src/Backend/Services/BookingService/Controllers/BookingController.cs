using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yandex_pract.MockDB;

namespace yandex_pract.Services.BookingService;

[ApiController, Route("events")]
public class BookingController(IBookingService bookingService) : Controller
{
	private readonly IEventDataBase _eventDataBase;

	[HttpPost("{eventId:guid}/book")]
	public IActionResult AddBooking([FromRoute] Guid eventId)
	{
		//проверить есть ли такое событие, если нет -404
		// иначе вызвать метод и вернуть 
		var (haveEvent, result) = _eventDataBase.GetEventById(eventId);
		if (!haveEvent)
			return NotFound();

		var booking = bookingService.CreateBookingAsync(eventId);

		var responseData = new { Id = booking.Id, EventId = booking, eventId, Status = booking.Status };
		Response.Headers.Append("Location", $"/bookings/{booking.Id}");

		return CreatedAtAction(nameof(AddBooking), responseData);
	}

	[HttpGet("bookings/{bookingId:guid}")]
	public IActionResult GetBooking([FromRoute] Guid bookingId)
	{
		var result = bookingService.GetBookingByIdAsync(bookingId);
		return null;
	}
}