
using BookingService.Domain.Models.BookingModel;

namespace BookingService.Application.Services.Abstraction.Repositories;

public interface IBookingRepository
{
	public Task AddBookingAsync(Domain.Models.BookingModel.BookingModel bookingModel);
	public Task DeleteBookingAsync(BookingModel booking);
	public Task<BookingModel?> GetBookingAsync(Guid bookingId);
	public Task SaveChangesAsync();
	public Task<List<BookingModel>> GetPendingAsync();
	public Task<int> GetActiveBookingsCountAsync(Guid userId);
}