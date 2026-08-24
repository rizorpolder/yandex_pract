using BookingService.Application.Services.Abstraction.Repositories;
using BookingService.Domain.Models.BookingModel;
using BookingService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Repositories;

public class EfBookingRepository(AppDbContext dbContext) : IBookingRepository
{
	public async Task UpdateBookingAsync(BookingModel bookingModel)
	{
		var existing = await dbContext.Bookings.FindAsync(bookingModel.Id);
		if (existing is null) return;

		dbContext.Entry(existing).CurrentValues.SetValues(bookingModel);
		await dbContext.SaveChangesAsync();
		dbContext.Entry(existing).State = EntityState.Detached;
	}

	public async Task AddBookingAsync(BookingModel bookingModel)
	{
		await dbContext.Bookings.AddAsync(bookingModel);
	}

	public Task DeleteBookingAsync(BookingModel bookingModel)
	{
		dbContext.Bookings.Remove(bookingModel);
		return Task.CompletedTask;
	}

	public async Task<BookingModel?> GetBookingAsync(Guid bookingId)
	{
		return await dbContext.Bookings.FirstOrDefaultAsync(booking => booking.Id.Equals(bookingId));
	}

	public async Task SaveChangesAsync()
	{
		await dbContext.SaveChangesAsync();
	}

	public async Task<List<BookingModel>> GetPendingAsync()
	{
		return await dbContext.Bookings.Where(x => x.Status.Equals(BookingStatus.Pending)).ToListAsync();
	}

	public async Task<int> GetActiveBookingsCountAsync(Guid userId)
	{
		return await dbContext.Bookings.AsNoTracking().Where(x => x.UserId.Equals(userId) &&
		                                                          (x.Status == BookingStatus.Pending ||
		                                                           x.Status == BookingStatus.Confirmed)).CountAsync();
	}
}