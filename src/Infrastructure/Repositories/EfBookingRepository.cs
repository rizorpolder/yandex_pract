using Application.Services.Abstraction.Repositories;
using Domain.Models.Bookings;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EfBookingRepository(AppDbContext dbContext) : IBookingRepository
{
	public async Task UpdateBookingAsync(Booking booking)
	{
		var existing = await dbContext.Bookings.FindAsync(booking.Id);
		if (existing is null) return;

		dbContext.Entry(existing).CurrentValues.SetValues(booking);
		await dbContext.SaveChangesAsync();
		dbContext.Entry(existing).State = EntityState.Detached;
	}

	public async Task AddBookingAsync(Booking booking)
	{
		await dbContext.Bookings.AddAsync(booking);
	}

	public Task DeleteBookingAsync(Booking booking)
	{
		dbContext.Bookings.Remove(booking);
		return Task.CompletedTask;
	}

	public async Task<Booking?> GetBookingAsync(Guid bookingId)
	{
		return await dbContext.Bookings.FirstOrDefaultAsync(booking => booking.Id.Equals(bookingId));
	}

	public async Task SaveChangesAsync()
	{
		await dbContext.SaveChangesAsync();
	}

	public async Task<List<Booking>> GetPendingAsync()
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