using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class PaginationTests
{
	private AppDbContext CreateDb()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
			.Options;

		return new AppDbContext(options);
	}

	private (AppDbContext db,
		IEventService eventService,
		IBookingService bookingService) CreateServices()
	{
		var db = CreateDb();

		var eventDb = new EfEventRepository(db);
		var bookingDb = new EfBookingRepository(db);

		var filter = new EventFilterService();

		var eventService = new EventService(eventDb, filter);
		var bookingService = new BookingService(bookingDb, eventDb);

		return (db, eventService, bookingService);
	}

	private void Cleanup(AppDbContext db)
	{
		db.Events.RemoveRange(db.Events);
		db.Bookings.RemoveRange(db.Bookings);
		db.SaveChanges();
	}

	[Fact]
	public async Task PaginationTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		
		for (int i = 0; i < 25; i++)
		{
			var evt = new Event($"title {i}", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 10);
			await eventService.CreateEventAsync(evt);
		}

		var page1 = await eventService.GetEvents(null, null, null, 1, 5);
		var page2 = await eventService.GetEvents(null, null, null, 2, 5);

		Assert.Equal(5, page1.Data.Count);
		Assert.Equal(5, page2.Data.Count);
		Assert.NotEqual(page1.Data.First().ID, page2.Data.First().ID);
	}
}