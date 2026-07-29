using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Abstraction.Repositories;
using Domain.Models.Booking;
using Microsoft.EntityFrameworkCore;

namespace yandex_pract.DbContext;

public class EfBookingRepository : IBookingRepository
{
	private readonly AppDbContext _dbContext;

	public EfBookingRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<bool> EnqueueAsync(Booking booking)
	{
		bool isSuccess = false;
		_dbContext.Bookings.Add(booking);
		isSuccess = await _dbContext.SaveChangesAsync() > 0;
		_dbContext.Entry(booking).State = EntityState.Detached;
		return isSuccess;
	}
	
	
	public async Task<(bool found, Booking booking)> TryFindBookingAsync(Guid bookingId)
	{
		var booking = await _dbContext.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(bookingId));
		return (booking != null, booking);
	}

	public async Task<List<Booking>> GetPendingAsync()
	{
		return await _dbContext.Bookings.AsNoTracking().Where(x => x.Status.Equals(BookingStatus.Pending))
			.ToListAsync();
	}

	public async Task UpdateBookingAsync(Booking booking)
	{
		var existing = await _dbContext.Bookings.FindAsync(booking.Id);
		if (existing is null) return;

		_dbContext.Entry(existing).CurrentValues.SetValues(booking);
		await _dbContext.SaveChangesAsync();
		_dbContext.Entry(existing).State = EntityState.Detached;
	}
}