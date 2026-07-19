using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Services.BookingService.Models;

namespace yandex_pract.DbContext;

public class EfBookingDataBase : IBookingDataBase
{
	private readonly AppDbContext _dbContext;

	public EfBookingDataBase(AppDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task EnqueueAsync(Booking booking)
	{
		_dbContext.Bookings.Add(booking);
		await _dbContext.SaveChangesAsync();
	}

	public async Task<(bool found, Booking booking)> TryFindBookingAsync(Guid bookingId)
	{
		var booking = await _dbContext.Bookings.FirstOrDefaultAsync(x => x.Id.Equals(bookingId));
		return (booking != null, booking);
	}

	public async Task<List<Booking>> GetPendingAsync()
	{
		return await _dbContext.Bookings.Where(x => x.Status.Equals(BookingStatus.Pending)).ToListAsync();
	}

	public async Task UpdateBookingAsync(Booking booking)
	{
		_dbContext.Bookings.Update(booking);
		await _dbContext.SaveChangesAsync();
	}
}