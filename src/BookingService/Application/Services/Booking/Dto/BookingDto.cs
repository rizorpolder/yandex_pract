using BookingService.Domain.Models.BookingModel;

namespace BookingService.Application.Services.Booking.Dto;

public class BookingDto
{
	public Guid Id;
	public Guid EventId;
	public Guid UserId;
	public BookingStatus Status;
	public DateTime CreatedAt;
	public DateTime ProcessedAt;
}