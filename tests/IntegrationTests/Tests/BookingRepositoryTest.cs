using System.Collections.Concurrent;
using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Domain.Exceptions;
using Domain.Models.Booking;
using Domain.Models.Event;
using Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using yandex_pract.CustomEventService;
using yandex_pract.DbContext;
using yandex_pract.Filters;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class BookingRepositoryTest(PostgresContainerFixture fixture) : ABaseTestRepository(fixture)
{
	protected override string[] TablesToTruncate =>
		["bookings", "events"];

	[Fact]
	public async Task CreateSingleBooking_ShouldCreateBookingAndDecreaseSeats()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddSeconds(10),
			3);

		Assert.True(await eventService.CreateEventAsync(evt));

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result);
		Assert.NotNull(booking);
		Assert.Equal(evt.Id, booking.EventId);

		var (_, updatedEvent) = await eventService.GetEventById(evt.Id);
		Assert.Equal(2, updatedEvent.AvailableSeats);
	}

	[Fact]
	public async Task CreateSeveralBookings_ShouldCreateUniqueBookings()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddSeconds(10),
			3);

		await eventService.CreateEventAsync(evt);

		var b1 = await bookingService.CreateBookingAsync(evt.Id);
		var b2 = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(b1.result);
		Assert.True(b2.result);

		Assert.NotEqual(b1.booking.Id, b2.booking.Id);
	}

	[Fact]
	public async Task GetBookingById_ShouldReturnBooking()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddSeconds(10),
			3);

		await eventService.CreateEventAsync(evt);

		var (_, booking) = await bookingService.CreateBookingAsync(evt.Id);

		var (found, loaded) = await bookingService.GetBookingByIdAsync(booking.Id);

		Assert.True(found);
		Assert.NotNull(loaded);
	}

	[Fact]
	public async Task CreateBooking_WithWrongEventId_ShouldReturnFalse()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingRepo = new EfBookingRepository(ctx);
		var eventRepo = new EfEventRepository(ctx);

		var bookingService = new BookingService(bookingRepo, eventRepo);

		var (result, booking) = await bookingService.CreateBookingAsync(Guid.NewGuid());

		Assert.False(result);
		Assert.Null(booking);
	}

	[Fact]
	public async Task CreateBooking_ForRemovedEvent_ShouldFail()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddSeconds(10),
			3);

		await eventService.CreateEventAsync(evt);
		await eventService.RemoveEvent(evt);

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.False(result);
		Assert.Null(booking);
	}

	[Fact]
	public async Task GetBooking_WithBrokenId_ShouldReturnFalse()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingRepo = new EfBookingRepository(ctx);
		var eventRepo = new EfEventRepository(ctx);

		var bookingService = new BookingService(bookingRepo, eventRepo);

		var (found, booking) = await bookingService.GetBookingByIdAsync(Guid.NewGuid());

		Assert.False(found);
		Assert.Null(booking);
	}

	[Fact]
	public async Task BackgroundBookingService_ShouldConfirmPendingBookings()
	{
		await ResetDatabaseAsync();

		var services = new ServiceCollection();

		services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_fixture.Postgres.GetConnectionString()));
		services.AddScoped<IEventRepository, EfEventRepository>();
		services.AddScoped<IBookingRepository, EfBookingRepository>();
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();

		var provider = services.BuildServiceProvider();

		var eventService = provider.GetRequiredService<IEventService>();
		var bookingService = provider.GetRequiredService<IBookingService>();
		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			3);

		await eventService.CreateEventAsync(evt);

		var (_, booking) = await bookingService.CreateBookingAsync(evt.Id);

		var worker = new BackgroundBookingService(scopeFactory);

		using var cts = new CancellationTokenSource();
		var task = worker.StartAsync(cts.Token);

		await Task.Delay(200);
		cts.Cancel();
		await task;

		var (found, updated) = await bookingService.GetBookingByIdAsync(booking.Id);

		Assert.True(found);
		Assert.Equal(BookingStatus.Confirmed, updated.Status);
		Assert.NotNull(updated.ProcessedAt);
	}

	[Fact]
	public async Task CreateBooking_ShouldDecreaseAvailableSeats()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			3);

		await eventService.CreateEventAsync(evt);

		var before = evt.AvailableSeats;

		await bookingService.CreateBookingAsync(evt.Id);

		var (_, updated) = await eventService.GetEventById(evt.Id);

		Assert.Equal(before - 1, updated.AvailableSeats);
	}

	[Fact]
	public async Task CreateSeveralBookings_UntilLimit_ShouldSucceed()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			3);

		await eventService.CreateEventAsync(evt);

		await bookingService.CreateBookingAsync(evt.Id);
		await bookingService.CreateBookingAsync(evt.Id);
		await bookingService.CreateBookingAsync(evt.Id);

		var (_, updated) = await eventService.GetEventById(evt.Id);
		Assert.Equal(0, updated.AvailableSeats);
	}

	[Fact]
	public async Task CreateBooking_WhenSeatsExhausted_ShouldThrow()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			1);

		await eventService.CreateEventAsync(evt);

		await bookingService.CreateBookingAsync(evt.Id);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
			bookingService.CreateBookingAsync(evt.Id));
	}

	[Fact]
	public async Task CreateBooking_ForNonExistingEvent_ShouldReturnFalse()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingRepo = new EfBookingRepository(ctx);
		var eventRepo = new EfEventRepository(ctx);

		var bookingService = new BookingService(bookingRepo, eventRepo);

		var (result, booking) = await bookingService.CreateBookingAsync(Guid.NewGuid());

		Assert.False(result);
		Assert.Null(booking);
	}

	[Fact]
	public async Task CreateBooking_WhenZeroSeats_ShouldThrow()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			0);

		await eventService.CreateEventAsync(evt);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
			bookingService.CreateBookingAsync(evt.Id));
	}

	[Fact]
	public async Task Booking_Confirm_ShouldSetStatusAndProcessedAt()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			3);

		await eventService.CreateEventAsync(evt);

		var (_, booking) = await bookingService.CreateBookingAsync(evt.Id);

		booking.Confirm();

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotNull(booking.ProcessedAt);
	}

	[Fact]
	public async Task Booking_Reject_ShouldReleaseSeats()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			1);

		await eventService.CreateEventAsync(evt);

		var (_, booking) = await bookingService.CreateBookingAsync(evt.Id);

		booking.Reject();

		var (_, updatedEvent) = await eventService.GetEventById(evt.Id);
		updatedEvent.ReleaseSeats();

		Assert.True(await eventService.TryUpdateEvent(updatedEvent));

		var (_, after) = await eventService.GetEventById(evt.Id);
		Assert.Equal(1, after.AvailableSeats);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_ShouldAllowNewBooking()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			1);

		await eventService.CreateEventAsync(evt);

		var (_, booking1) = await bookingService.CreateBookingAsync(evt.Id);

		booking1.Reject();

		var (_, updatedEvent) = await eventService.GetEventById(evt.Id);
		updatedEvent.ReleaseSeats();

		Assert.True(await eventService.TryUpdateEvent(updatedEvent));

		var (result2, booking2) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result2);
		Assert.NotNull(booking2);
	}

	[Fact]
	public async Task ConcurrentBookings_ShouldNotOverbook()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var totalSeats = 5;

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			totalSeats);

		await eventService.CreateEventAsync(evt);

		var successes = 0;
		var exceptions = 0;

		var tasks = Enumerable.Range(0, 20).Select(async _ =>
		{
			try
			{
				var (result, _) = await bookingService.CreateBookingAsync(evt.Id);
				if (result) Interlocked.Increment(ref successes);
			}
			catch (NoAvailableSeatsException)
			{
				Interlocked.Increment(ref exceptions);
			}
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, successes);
		Assert.Equal(20 - totalSeats, exceptions);
	}

	[Fact]
	public async Task ConcurrentBookings_ShouldGenerateUniqueIds()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var totalSeats = 10;

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			totalSeats);

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
	}
}