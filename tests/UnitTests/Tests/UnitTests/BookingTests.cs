using System.Collections.Concurrent;
using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Domain.Exceptions;
using Domain.Models.Booking;
using Domain.Models.Event;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using yandex_pract.CustomEventService;
using yandex_pract.DbContext;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace EventTests.Tests;

public class BookingTests
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
	public async Task CreateSingleBookingTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await eventService.CreateEventAsync(evt);
		Assert.True(added);


		var bookingResult = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(bookingResult.result);
		Assert.NotNull(bookingResult.booking);
		Assert.Equal(evt.Id, bookingResult.booking.EventId);
		Cleanup(db);
	}

	[Fact]
	public async Task CreateSeveralBookingsTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await eventService.CreateEventAsync(evt);
		Assert.True(added);

		var booking1 = await bookingService.CreateBookingAsync(evt.Id);
		var booking2 = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(booking1.result);
		Assert.True(booking2.result);
		Assert.NotNull(booking1.booking);
		Assert.NotNull(booking2.booking);
		Assert.Equal(evt.Id, booking1.booking.EventId);
		Assert.Equal(evt.Id, booking2.booking.EventId);
		Assert.NotEqual(booking1.booking.Id, booking2.booking.Id);
		Cleanup(db);
	}

	[Fact]
	public async Task GetBookingByIdTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await eventService.CreateEventAsync(evt);
		Assert.True(added);
		var booking = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(booking.result);
		Assert.NotNull(booking.booking);

		var result = await bookingService.GetBookingByIdAsync(booking.booking.Id);
		Assert.True(result.haveBooking);
		Assert.NotNull(result.booking);
		Cleanup(db);
	}

	[Fact]
	public async Task CreateBookingWithWrongIdTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var booking = await bookingService.CreateBookingAsync(Guid.NewGuid());
		Assert.False(booking.result);
		Assert.Null(booking.booking);
		Cleanup(db);
	}

	[Fact]
	public async Task CreateBookingForRemovedEventTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		await eventService.CreateEventAsync(evt);

		var removed = await eventService.RemoveEvent(evt);

		Assert.True(removed);

		var result = await bookingService.CreateBookingAsync(evt.Id);

		Assert.False(result.result);
		Assert.Null(result.booking);
		Cleanup(db);
	}

	[Fact]
	public async Task GetBookingWithBrokenIdTest()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var booking = new Booking(evt.Id);
		var result = await bookingService.GetBookingByIdAsync(booking.Id);
		Assert.False(result.haveBooking);
		Assert.Null(result.booking);
		Cleanup(db);
	}

	[Fact]
	public async Task BookingStatusChangesAfterBackgroundProcessing()
	{
		var services = new ServiceCollection();

		var dbName = "TestDb_" + Guid.NewGuid();

		services.AddDbContext<AppDbContext>(options =>
			options.UseInMemoryDatabase(dbName));

		services.AddScoped<IEventRepository, EfEventRepository>();
		services.AddScoped<IBookingRepository, EfBookingRepository>();
		services.AddScoped<EventFilterService>();

		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();

		var provider = services.BuildServiceProvider();

		var db = provider.GetRequiredService<AppDbContext>();
		var eventService = provider.GetRequiredService<IEventService>();
		var bookingService = provider.GetRequiredService<IBookingService>();
		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
		
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await eventService.CreateEventAsync(evt);

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(result);
		Assert.Equal(BookingStatus.Pending, booking.Status);

		var bookingResult = await bookingService.GetBookingByIdAsync(booking.Id);
		
		var worker = new BackgroundBookingService(scopeFactory);

		using var cts = new CancellationTokenSource();
		var workerTask = worker.StartAsync(cts.Token);

		await Task.Delay(100, cts.Token);

		await cts.CancelAsync();
		await workerTask;

		var (found, updated) = await bookingService.GetBookingByIdAsync(booking.Id);

		Assert.True(found);
		Assert.Equal(BookingStatus.Confirmed, updated.Status);
		Assert.NotNull(updated.ProcessedAt);
		Cleanup(db);
	}


	[Fact]
	public async Task CreateBooking_DecreasesAvailableSeats()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await eventService.CreateEventAsync(evt);

		var before = evt.AvailableSeats;

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result);
		Assert.NotNull(booking);

		var (found, updatedEvent) = await eventService.GetEventById(evt.Id);
		Assert.True(found);
		Assert.Equal(before - 1, updatedEvent.AvailableSeats);
		Cleanup(db);
	}

	[Fact]
	public async Task CreateSeveralBookings_UntilLimit_AllSuccessful()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await eventService.CreateEventAsync(evt);

		var (r1, b1) = await bookingService.CreateBookingAsync(evt.Id);
		var (r2, b2) = await bookingService.CreateBookingAsync(evt.Id);
		var (r3, b3) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(r1);
		Assert.True(r2);
		Assert.True(r3);

		Assert.NotNull(b1);
		Assert.NotNull(b2);
		Assert.NotNull(b3);

		Assert.NotEqual(b1.Id, b2.Id);
		Assert.NotEqual(b2.Id, b3.Id);
		Assert.NotEqual(b1.Id, b3.Id);

		var (_, updatedEvent) = await eventService.GetEventById(evt.Id);
		Assert.Equal(0, updatedEvent.AvailableSeats);
		Cleanup(db);
	}

	[Fact]
	public async Task CreateBooking_WhenSeatsExhausted_ThrowsNoAvailableSeatsException()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		await eventService.CreateEventAsync(evt);

		var first = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(first.result);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(evt.Id));
		Cleanup(db);
	}

	[Fact]
	public async Task CreateBooking_ForNonExistingEvent_ReturnsFalse()
	{
		var (db, eventService, bookingService) = CreateServices();

		var id = Guid.NewGuid();

		var (result, booking) = await bookingService.CreateBookingAsync(id);

		Assert.False(result);
		Assert.Null(booking);
		Cleanup(db);
	}

	[Fact]
	public async Task CreateBooking_NoSeatsLeft_ThrowsNoAvailableSeatsException()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 0);
		await eventService.CreateEventAsync(evt);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>  bookingService.CreateBookingAsync(evt.Id));
		Cleanup(db);
	}

	[Fact]
	public async Task Booking_Confirm_SetsStatusAndProcessedAt()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await eventService.CreateEventAsync(evt);

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result);

		booking.Confirm();

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotNull(booking.ProcessedAt);
		Cleanup(db);
	}

	[Fact]
	public async Task Booking_Reject_SetsStatusAndProcessedAt()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await eventService.CreateEventAsync(evt);

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result);
		booking.Reject();

		Assert.Equal(BookingStatus.Rejected, booking.Status);
		Assert.NotNull(booking.ProcessedAt);
		Cleanup(db);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_RestoresAvailableSeats()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		await eventService.CreateEventAsync(evt);

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result);
		var (_, updatedEventBefore) = await eventService.GetEventById(evt.Id);
		Assert.Equal(0, updatedEventBefore.AvailableSeats);

		booking.Reject();
		updatedEventBefore.ReleaseSeats();

		bool isUpdated = await eventService.TryUpdateEvent(updatedEventBefore);
		Assert.True(isUpdated);

		var (_, updatedEventAfter) = await eventService.GetEventById(evt.Id);
		Assert.Equal(1, updatedEventAfter.AvailableSeats);
		Cleanup(db);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_AllowsNewBooking()
	{
		var (db, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		await eventService.CreateEventAsync(evt);

		var (result1, booking1) = await bookingService.CreateBookingAsync(evt.Id);
		
		Assert.True(result1);
		
		var (hasEvt, evtData)= await eventService.GetEventById(evt.Id);
		
		Assert.True(hasEvt);

		booking1.Reject();
		evtData.ReleaseSeats();

		var isUpdated = await eventService.TryUpdateEvent( evtData);
		Assert.True(isUpdated);
		
		var (result2, booking2) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result2);
		Assert.NotNull(booking2);
		Assert.Equal(evt.Id, booking2.EventId);
		Cleanup(db);
	}

	[Fact]
	public async Task ConcurrentBookings_NoOverbookingOccurs()
	{
		var (db, eventService, bookingService) = CreateServices();

		var totalSeats = 5;

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);
		await eventService.CreateEventAsync(evt);

		var exceptions = 0;
		var successes = 0;

		var tasks = Enumerable.Range(0, 20).Select(async _ =>
		{
			try
			{
				var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);
				if (result)
					Interlocked.Increment(ref successes);
			}
			catch (NoAvailableSeatsException)
			{
				Interlocked.Increment(ref exceptions);
			}
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, successes);
		Assert.Equal(20 - totalSeats, exceptions);

		var (haveElement, updatedEvent) = await eventService.GetEventById(evt.Id);

		Assert.True(haveElement);
		Assert.Equal(0, updatedEvent.AvailableSeats);
		Cleanup(db);
	}


	[Fact]
	public async Task ConcurrentBookings_AllIdsAreUnique()
	{
		var (db, eventService, bookingService) = CreateServices();

		var totalSeats = 10;

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);
		await eventService.CreateEventAsync(evt);

		var ids = new ConcurrentBag<Guid>();

		var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
		{
			var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

			Assert.True(result);
			ids.Add(booking.Id);
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, ids.Count);
		Assert.Equal(totalSeats, ids.Distinct().Count());
		Cleanup(db);
	}
}