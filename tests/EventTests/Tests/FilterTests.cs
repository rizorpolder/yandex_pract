using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.DbContext;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace EventTests.Tests;

public class FilterTests
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

		var eventDb = new EfEventDataBase(db);
		var bookingDb = new EfBookingDataBase(db);

		var filter = new EventFilterService();

		var eventService = new EventService(eventDb, filter);
		var bookingService = new BookingService(db, eventDb);

		return (db, eventService, bookingService);
	}

	private void Cleanup(AppDbContext db)
	{
		db.Events.RemoveRange(db.Events);
		db.Bookings.RemoveRange(db.Bookings);
		db.SaveChanges();
	}

	[Fact]
	public async Task TitleFilterTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var result = await eventService.GetEvents("meeting", null, null, 1, 10);

		Assert.All(result.Data,
			e =>
				Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public async Task DateFilterTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var from = new DateTime(2024, 1, 1);
		var to = new DateTime(2024, 12, 31);

		var result = await eventService.GetEvents(null, from, to, 1, 10);

		Assert.All(result.Data,
			e =>
			{
				Assert.True(e.StartAt >= from);
				Assert.True(e.EndAt <= to);
			});
	}

	[Fact]
	public async Task CombinedFilterTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var result = await eventService.GetEvents("meeting",
			new DateTime(2024, 1, 1),
			new DateTime(2024, 12, 31),
			1,
			10);

		Assert.All(result.Data,
			e =>
			{
				Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase);
				Assert.True(e.StartAt >= new DateTime(2024, 1, 1));
				Assert.True(e.EndAt <= new DateTime(2024, 12, 31));
			});
	}
}