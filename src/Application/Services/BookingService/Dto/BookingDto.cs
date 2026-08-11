using Domain.Models.Booking;

namespace Application.Services.BookingService.Dto;

public class BookingDto
{
	public Guid Id;
	public Guid EventId;
	public BookingStatus Status;
	public DateTime CreatedAt;
	public DateTime ProcessedAt;
}