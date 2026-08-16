using Application.Services.BookingService.Dto;
using Domain.Models.Bookings;

namespace Application.Services.Mapping;

internal static class BookingMapper
{
	public static BookingDto ToDto(Booking booking)
	{
		return new BookingDto()
		{
			Id = booking.Id,
			UserId = booking.UserId,
			EventId = booking.EventId,
			Status = booking.Status,
			CreatedAt = booking.CreatedAt,
			ProcessedAt = booking.ProcessedAt,
		};
	}

	public static Booking FromDto(BookingDto booking)
	{
		return new Booking(booking.EventId, booking.UserId)
		{
			Status = booking.Status,
			CreatedAt = booking.CreatedAt,
			ProcessedAt = booking.ProcessedAt,
		};
	}
}