using Application.Services.Abstraction.Services;
using Application.Services.BookingService;
using Domain.Models.Event;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.DbContext;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace EventTests.Tests;

public class CrudTests
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
	public async Task CreateEventTest()
	{
		var (db, eventService, bookingService) = CreateServices();
		var evt = new Event(
			"testTitle",
			"testDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(10), 3);

		var added = await eventService.CreateEventAsync(evt);

		Assert.True(added);

		var result = await eventService.GetEventById(evt.Id);
		Assert.True(result.hasElement);
		Assert.NotNull(result.resultModel);
		Assert.Equal(evt.Title, result.resultModel!.Title);
		Assert.Equal(evt.Description, result.resultModel.Description);
	}

	[Fact]
	public async Task GetAllEventsTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt1 = new Event("A", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		var evt2 = new Event("B", "desc", DateTime.Now, DateTime.Now.AddMinutes(2), 5);

		await eventService.CreateEventAsync(evt1);
		await eventService.CreateEventAsync(evt2);

		var result = await eventService.GetEvents(null, null, null, 1, 10);


		Assert.NotNull(result);
		Assert.Equal(2, result.Data.Count);
		Assert.Contains(result.Data, e => e.Title == "A");
		Assert.Contains(result.Data, e => e.Title == "B");
	}

	[Fact]
	public async Task GetEventByID()
	{
		var (db, eventService, bookingService) = CreateServices();

		var testEvt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await eventService.CreateEventAsync(testEvt);
		Assert.True(added);

		var evt = await eventService.GetEventById(testEvt.Id);

		Assert.True(evt.hasElement);
		Assert.NotNull(evt.resultModel);
		Assert.Equal(testEvt.Id, evt.resultModel!.Id);
	}

	[Fact]
	public async Task GetEventByIncorrectID()
	{
		var (db, eventService, bookingService) = CreateServices();

		var id = Guid.NewGuid();

		var evt = await eventService.GetEventById(id);

		Assert.False(evt.hasElement);
		Assert.Null(evt.resultModel);
	}

	[Fact]
	public async Task UpdateEventTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event(
			"oldTitle",
			"oldDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(10), 3);

		await eventService.CreateEventAsync(evt);

		var updated = new Event(
			"newTitle",
			"newDescription",
			DateTime.Now,
			DateTime.Now.AddSeconds(20), 3);
		updated.SetGuid(evt.Id);
		
		var result = await eventService.TryUpdateEvent(updated);
		Assert.True(result);
		
		var (hasElement, eventData) = await eventService.GetEventById(evt.Id);
		Assert.True(hasElement);
		Assert.NotNull(eventData);
		Assert.Equal("newTitle", eventData.Title);
		Assert.Equal("newDescription", eventData.Description);
	}

	[Fact]
	public async Task UpdateBrokenIDEventTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event(
			"title",
			"desc",
			DateTime.Now,
			DateTime.Now.AddSeconds(10), 3);
		evt.SetGuid(Guid.NewGuid());
		var result = await eventService.TryUpdateEvent( evt);
		Assert.False(result);
	}

	[Fact]
	public async Task DeleteEventTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		await eventService.CreateEventAsync(evt);
		
		var removed = await eventService.RemoveEvent(evt);

		Assert.True(removed);

		var result = await eventService.GetEventById(evt.Id);
		Assert.False(result.hasElement);
		Assert.Null(result.resultModel);
	}
}