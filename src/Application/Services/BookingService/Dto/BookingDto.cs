using Domain.Models.Bookings;

namespace Application.Services.BookingService.Dto;

public class BookingDto
{
	public Guid Id;
	public Guid EventId;
	public Guid UserId;
	public BookingStatus Status;
	public DateTime CreatedAt;
	public DateTime ProcessedAt;
}