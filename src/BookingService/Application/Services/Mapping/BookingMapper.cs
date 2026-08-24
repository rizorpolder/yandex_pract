using BookingService.Application.Services.Booking.Dto;
using BookingService.Domain.Models.BookingModel;

namespace BookingService.Application.Services.Mapping;

internal static class BookingMapper
{
	public static BookingDto ToDto(BookingModel booking)
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

	public static BookingModel FromDto(BookingDto booking)
	{
		return new BookingModel(booking.EventId, booking.UserId)
		{
			Status = booking.Status,
			CreatedAt = booking.CreatedAt,
			ProcessedAt = booking.ProcessedAt,
		};
	}
}