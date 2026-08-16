using Application.Services.Abstraction.Services;
using Domain.Exceptions;
using Domain.Models.Users;
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
				return NotFound(new { Message = bookingResult.ErrorMessage });

			var location = $"/events/bookings/{bookingResult.Value.Id}";
			Response.Headers.Append("Location", location);

			return Accepted(bookingResult.Value);
		}
		catch (NoAvailableSeatsException e)
		{
			return Conflict(e.Message);
		}
		catch (BookingLimitReachedException e)
		{
			return Conflict(e.Message);
		}
		catch (OutOfDateException e)
		{
			return BadRequest(e.Message);
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

	[HttpDelete("bookings/{bookingId:guid}", Name = nameof(RemoveBooking))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
	public async Task<IActionResult> RemoveBooking(Guid bookingId)
	{
		var userId = Guid.Parse(User.FindFirst("id")!.Value);
		var role = Enum.Parse<UserRole>(User.FindFirst("Role")!.Value, true);
		try
		{
			var result = await bookingService.CancelBookingAsync(bookingId, userId, role);
			if (!result.IsSuccess)
				return NotFound(result.ErrorMessage);
			return Ok(result.Value);
		}
		catch (PermissionException e)
		{
			return Forbid(e.Message);
		}
	}
}