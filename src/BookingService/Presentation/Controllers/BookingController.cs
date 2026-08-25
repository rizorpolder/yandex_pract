using BookingService.Application.Services.Abstraction.Services;
using BookingService.Domain.Exceptions;
using Common.Models;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
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
	[Authorize]
	public async Task<IActionResult> AddBooking(Guid eventId)
	{
		try
		{
			var userId = Guid.Parse(User.FindFirst("id")!.Value);
			var bookingResult = await bookingService.CreateBookingAsync(eventId, userId);

			if (!bookingResult.IsSuccess)
				return NotFound(new {Message = bookingResult.ErrorMessage});

			var location = $"/events/bookings/{bookingResult.Value.Id}";
			Response.Headers.Append("Location", location);

			return Accepted(bookingResult.Value);
		}
		catch (BookingLimitReachedException e)
		{
			return Conflict(e.Message);
		}
	}

	[HttpGet("bookings/{bookingId:guid}", Name = nameof(GetBooking))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	[Authorize]
	public async Task<IActionResult> GetBooking(Guid bookingId)
	{
		var bookingResult = await bookingService.GetBookingByIdAsync(bookingId);

		if (!bookingResult.IsSuccess)
			return NotFound();

		var userId = Guid.Parse(User.FindFirst("id")!.Value);
		var role = Enum.Parse<UserRole>(User.FindFirst("role")!.Value);

		if (bookingResult.Value != null && bookingResult.Value.UserId != userId && role != UserRole.Admin)
			return Forbid();

		return Ok(bookingResult.Value);
	}

	[HttpDelete("bookings/{bookingId:guid}", Name = nameof(RemoveBooking))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
	[Authorize]
	public async Task<IActionResult> RemoveBooking(Guid bookingId)
	{
		var userId = Guid.Parse(User.FindFirst("id")!.Value);
		var role = Enum.Parse<UserRole>(User.FindFirst("role")!.Value);

		var result = await bookingService.CancelBookingAsync(bookingId, userId, role);
		if (!result.IsSuccess)
			return result.ErrorMessage switch
			{
				"NotFound" => NotFound(),
				"Forbidden" => Forbid(),
				_ => BadRequest(new {Message = result.ErrorMessage})
			};

		return Ok(result.Value);
	}
}