using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.CustomException;
using yandex_pract.DbContext;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Services.BackgroundBookingService;
using yandex_pract.Services.BookingService;
using yandex_pract.Services.BookingService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class BookingTests
{
	private readonly IBookingService _service;
	private readonly IEventService _eventService;

	public BookingTests(TestDbFixture fixture)
	{
		_service = fixture.BookingService;
		_eventService = fixture.EventService;
	}

	[Fact]
	public async Task CreateSingleBookingTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await _eventService.CreateEventAsync(evt);
		Assert.True(added);


		var bookingResult = await _service.CreateBookingAsync(evt.Id);
		Assert.True(bookingResult.result);
		Assert.NotNull(bookingResult.booking);
		Assert.Equal(evt.Id, bookingResult.booking.EventId);
	}

	[Fact]
	public async Task CreateSeveralBookingsTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await _eventService.CreateEventAsync(evt);
		Assert.True(added);

		var booking1 = await _service.CreateBookingAsync(evt.Id);
		var booking2 = await _service.CreateBookingAsync(evt.Id);

		Assert.True(booking1.result);
		Assert.True(booking2.result);
		Assert.NotNull(booking1.booking);
		Assert.NotNull(booking2.booking);
		Assert.Equal(evt.Id, booking1.booking.EventId);
		Assert.Equal(evt.Id, booking2.booking.EventId);
		Assert.NotEqual(booking1.booking.Id, booking2.booking.Id);
	}

	[Fact]
	public async Task GetBookingByIdTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = await _eventService.CreateEventAsync(evt);
		Assert.True(added);
		var booking = await _service.CreateBookingAsync(evt.Id);
		Assert.True(booking.result);
		Assert.NotNull(booking.booking);

		var result = await _service.GetBookingByIdAsync(booking.booking.Id);
		Assert.True(result.haveBooking);
		Assert.NotNull(result.booking);
	}

	[Fact]
	public async Task CreateBookingWithWrongIdTest()
	{
		var booking = await _service.CreateBookingAsync(Guid.NewGuid());
		Assert.False(booking.result);
		Assert.Null(booking.booking);
	}

	[Fact]
	public async Task CreateBookingForRemovedEventTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		await _eventService.CreateEventAsync(evt);

		var removed = await _eventService.RemoveEvent(evt);

		Assert.True(removed);

		var result = await _service.CreateBookingAsync(evt.Id);

		Assert.False(result.result);
		Assert.Null(result.booking);
	}

	[Fact]
	public async Task GetBookingWithBrokenIdTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var booking = new Booking(evt.Id);
		var result = await _service.GetBookingByIdAsync(booking.Id);
		Assert.False(result.haveBooking);
		Assert.Null(result.booking);
	}

	[Fact]
	public async Task BookingStatusChangesAfterBackgroundProcessing()
	{
		var services = new ServiceCollection();

		services.AddDbContext<AppDbContext>(options =>
			options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

		services.AddScoped<IEventDataBase, EfEventDataBase>();
		services.AddScoped<IBookingDataBase, EfBookingDataBase>();

		services.AddScoped<EventService>();
		services.AddScoped<BookingService>();

		services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());

		var provider = services.BuildServiceProvider();

		var eventService = provider.GetRequiredService<EventService>();
		var bookingService = provider.GetRequiredService<BookingService>();
		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await eventService.CreateEventAsync(evt);

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(result);
		Assert.Equal(BookingStatus.Pending, booking.Status);

		var worker = new BackgroundBookingService(scopeFactory);

		using var cts = new CancellationTokenSource();
		var workerTask = worker.StartAsync(cts.Token);

		await Task.Delay(50, cts.Token);

		await cts.CancelAsync();
		await workerTask;

		var (found, updated) = await bookingService.GetBookingByIdAsync(booking.Id);

		Assert.True(found);
		Assert.Equal(BookingStatus.Confirmed, updated.Status);
		Assert.NotNull(updated.ProcessedAt);
	}


	[Fact]
	public async Task CreateBooking_DecreasesAvailableSeats()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await _eventService.CreateEventAsync(evt);

		var before = evt.AvailableSeats;

		var (result, booking) = await _service.CreateBookingAsync(evt.Id);

		Assert.True(result);
		Assert.NotNull(booking);

		var (found, updatedEvent) = await _eventService.GetEventById(evt.Id);
		Assert.True(found);
		Assert.Equal(before - 1, updatedEvent.AvailableSeats);
	}

	[Fact]
	public async Task CreateSeveralBookings_UntilLimit_AllSuccessful()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await _eventService.CreateEventAsync(evt);

		var b1 = _service.CreateBookingAsync(evt.Id).Result.booking;
		var b2 = _service.CreateBookingAsync(evt.Id).Result.booking;
		var b3 = _service.CreateBookingAsync(evt.Id).Result.booking;

		Assert.NotNull(b1);
		Assert.NotNull(b2);
		Assert.NotNull(b3);

		Assert.NotEqual(b1.Id, b2.Id);
		Assert.NotEqual(b2.Id, b3.Id);
		Assert.NotEqual(b1.Id, b3.Id);

		var (_, updatedEvent) = await _eventService.GetEventById(evt.Id);
		Assert.Equal(0, updatedEvent.AvailableSeats);
	}

	[Fact]
	public async Task CreateBooking_WhenSeatsExhausted_ThrowsNoAvailableSeatsException()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		await _eventService.CreateEventAsync(evt);

		var first = await _service.CreateBookingAsync(evt.Id);
		Assert.True(first.result);

		Assert.Throws<NoAvailableSeatsException>(() => { _service.CreateBookingAsync(evt.Id).Wait(); });
	}

	[Fact]
	public async Task CreateBooking_ForNonExistingEvent_ReturnsFalse()
	{
		var id = Guid.NewGuid();

		var (result, booking) = await _service.CreateBookingAsync(id);

		Assert.False(result);
		Assert.Null(booking);
	}

	[Fact]
	public async Task CreateBooking_NoSeatsLeft_ThrowsNoAvailableSeatsException()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 0);
		await _eventService.CreateEventAsync(evt);

		Assert.Throws<NoAvailableSeatsException>(() => { _service.CreateBookingAsync(evt.Id).Wait(); });
	}

	[Fact]
	public async Task Booking_Confirm_SetsStatusAndProcessedAt()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await _eventService.CreateEventAsync(evt);

		var (result, booking) = await _service.CreateBookingAsync(evt.Id);

		Assert.True(result);

		booking.Confirm();

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotNull(booking.ProcessedAt);
	}

	[Fact]
	public async Task Booking_Reject_SetsStatusAndProcessedAt()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		await _eventService.CreateEventAsync(evt);

		var (result, booking) = await _service.CreateBookingAsync(evt.Id);

		Assert.True(result);
		booking.Reject();

		Assert.Equal(BookingStatus.Rejected, booking.Status);
		Assert.NotNull(booking.ProcessedAt);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_RestoresAvailableSeats()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		await _eventService.CreateEventAsync(evt);

		var (result, booking) = await _service.CreateBookingAsync(evt.Id);

		Assert.True(result);
		var (_, updatedEventBefore) = await _eventService.GetEventById(evt.Id);
		Assert.Equal(0, updatedEventBefore.AvailableSeats);

		booking.Reject();
		evt.ReleaseSeats();

		var (_, updatedEventAfter) = await _eventService.GetEventById(evt.Id);
		Assert.Equal(1, updatedEventAfter.AvailableSeats);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_AllowsNewBooking()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		await _eventService.CreateEventAsync(evt);

		var (result1, booking1) = await _service.CreateBookingAsync(evt.Id);

		Assert.True(result1);
		booking1.Reject();
		evt.ReleaseSeats();

		var (result2, booking2) = await _service.CreateBookingAsync(evt.Id);

		Assert.True(result2);
		Assert.NotNull(booking2);
		Assert.Equal(evt.Id, booking2.EventId);
	}

	[Fact]
	public async Task ConcurrentBookings_NoOverbookingOccurs()
	{
		var totalSeats = 5;

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);
		await _eventService.CreateEventAsync(evt);

		var exceptions = 0;
		var successes = 0;

		var tasks = Enumerable.Range(0, 20).Select(async _ =>
		{
			try
			{
				var (result, booking) = await _service.CreateBookingAsync(evt.Id);
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

		var (haveElement, updatedEvent) = await _eventService.GetEventById(evt.Id);

		Assert.True(haveElement);
		Assert.Equal(0, updatedEvent.AvailableSeats);
	}


	[Fact]
	public async Task ConcurrentBookings_AllIdsAreUnique()
	{
		var totalSeats = 10;

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);
		await _eventService.CreateEventAsync(evt);

		var ids = new ConcurrentBag<Guid>();

		var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
		{
			var (result, booking) = await _service.CreateBookingAsync(evt.Id);

			Assert.True(result);
			ids.Add(booking.Id);
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, ids.Count);
		Assert.Equal(totalSeats, ids.Distinct().Count());
	}
}