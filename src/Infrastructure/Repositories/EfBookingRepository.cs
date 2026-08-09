using Application.Services.Abstraction.Repositories;
using Domain.Models.Booking;
using Microsoft.EntityFrameworkCore;
using yandex_pract.DbContext;

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
		return await dbContext.Bookings.AsNoTracking().Where(x => x.Status.Equals(BookingStatus.Pending)).ToListAsync();
	}
}