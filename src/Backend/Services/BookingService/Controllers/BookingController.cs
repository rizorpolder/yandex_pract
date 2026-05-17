using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using yandex_pract.MockDB;

namespace yandex_pract.Services.BookingService;

[ApiController, Route("events")]
public class BookingController(IBookingService bookingService) : Controller
{
	private readonly IEventDataBase _eventDataBase;

	[HttpPost("{eventId:guid}/book")]
	public async Task<IActionResult> AddBooking([FromRoute] Guid eventId)
	{
		//проверить есть ли такое событие, если нет -404
		// иначе вызвать метод и вернуть 
		// var bookingAsync = await bookingService.CreateBookingAsync(eventId);
		return Ok();
	}

	[HttpGet("bookings/{bookingId:guid}")]
	public IActionResult GetBooking([FromRoute] Guid bookingId)
	{
		var result = bookingService.GetBookingByIdAsync(bookingId);
		return null;
	}
}