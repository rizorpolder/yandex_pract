using System.Collections.Concurrent;
using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.EventService.Dto;
using Application.Services.Filters;
using Domain.Exceptions;
using Domain.Models.Bookings;
using Domain.Models.Events;
using Domain.Models.Users;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class BookingRepositoryTest(PostgresContainerFixture fixture) : ABaseTestRepository(fixture)
{
	protected override string[] TablesToTruncate =>
		["bookings", "events", "users"];

	private static EventDto ToDto(Event evt) => new EventDto
	{
		Title = evt.Title,
		Description = evt.Description,
		StartAt = evt.StartAt,
		EndAt = evt.EndAt,
		TotalSeats = evt.TotalSeats
	};

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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddSeconds(10),
			DateTime.UtcNow.AddSeconds(20),
			3);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var bookingResult = await bookingService.CreateBookingAsync(eventId, user.Id);

		Assert.True(bookingResult.IsSuccess);
		Assert.NotNull(bookingResult.Value);
		Assert.Equal(eventId, bookingResult.Value.EventId);

		var updatedEvent = await eventService.GetEventById(eventId);
		Assert.True(updatedEvent.IsSuccess);
		Assert.Equal(2, updatedEvent.Value.AvailableSeats);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddSeconds(10),
			DateTime.UtcNow.AddSeconds(20),
			3);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var b1 = await bookingService.CreateBookingAsync(eventId, user.Id);
		var b2 = await bookingService.CreateBookingAsync(eventId, user.Id);

		Assert.True(b1.IsSuccess);
		Assert.True(b2.IsSuccess);

		Assert.NotEqual(b1.Value.Id, b2.Value.Id);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddSeconds(10),
			DateTime.UtcNow.AddSeconds(20),
			3);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);

		var booking = await bookingService.CreateBookingAsync(created.Value.ID, user.Id);
		Assert.True(booking.IsSuccess);

		var loaded = await bookingService.GetBookingByIdAsync(booking.Value.Id);

		Assert.True(loaded.IsSuccess);
		Assert.NotNull(loaded.Value);
	}

	[Fact]
	public async Task CreateBooking_WithWrongEventId_ShouldReturnFalse()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingRepo = new EfBookingRepository(ctx);
		var eventRepo = new EfEventRepository(ctx);

		var bookingService = new BookingService(bookingRepo, eventRepo);

		var result = await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddSeconds(10),
			3);
		var userId = Guid.NewGuid();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var removed = await eventService.RemoveEvent(created.Value);
		Assert.True(removed.IsSuccess);

		var result = await bookingService.CreateBookingAsync(eventId, userId);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task GetBooking_WithBrokenId_ShouldReturnFalse()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingRepo = new EfBookingRepository(ctx);
		var eventRepo = new EfEventRepository(ctx);

		var bookingService = new BookingService(bookingRepo, eventRepo);

		var result = await bookingService.GetBookingByIdAsync(Guid.NewGuid());

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task BackgroundBookingService_ShouldConfirmPendingBookings()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var services = new ServiceCollection();

		services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_fixture.Postgres.GetConnectionString()));
		services.AddScoped<IEventRepository, EfEventRepository>();
		services.AddScoped<IBookingRepository, EfBookingRepository>();
		services.AddScoped<EventFilterService>();
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();

		var provider = services.BuildServiceProvider();

		using var setupScope = provider.CreateScope();
		var eventService = setupScope.ServiceProvider.GetRequiredService<IEventService>();
		var bookingService = setupScope.ServiceProvider.GetRequiredService<IBookingService>();
		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			3);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);

		var booking = await bookingService.CreateBookingAsync(created.Value.ID, user.Id);
		Assert.True(booking.IsSuccess);

		var worker = new BackgroundBookingService(scopeFactory);

		await worker.StartAsync(CancellationToken.None);

		BookingStatus? finalStatus = null;
		DateTime? processedAt = null;

		for (var attempt = 0; attempt < 50; attempt++)
		{
			using var pollScope = provider.CreateScope();
			var pollBookingService = pollScope.ServiceProvider.GetRequiredService<IBookingService>();

			var polled = await pollBookingService.GetBookingByIdAsync(booking.Value.Id);
			Assert.True(polled.IsSuccess);

			if (polled.Value.Status != BookingStatus.Pending)
			{
				finalStatus = polled.Value.Status;
				processedAt = polled.Value.ProcessedAt;
				break;
			}

			await Task.Delay(100);
		}

		await worker.StopAsync(CancellationToken.None);

		Assert.Equal(BookingStatus.Confirmed, finalStatus);
		Assert.NotEqual(default, processedAt);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			3);
		var userId = Guid.NewGuid();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var before = created.Value.AvailableSeats;

		await bookingService.CreateBookingAsync(eventId, userId);

		var updated = await eventService.GetEventById(eventId);

		Assert.True(updated.IsSuccess);
		Assert.Equal(before - 1, updated.Value.AvailableSeats);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(10),
			3);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		await bookingService.CreateBookingAsync(eventId, user.Id);
		await bookingService.CreateBookingAsync(eventId, user.Id);
		await bookingService.CreateBookingAsync(eventId, user.Id);

		var updated = await eventService.GetEventById(eventId);
		Assert.True(updated.IsSuccess);
		Assert.Equal(0, updated.Value.AvailableSeats);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			1);
		var userId = Guid.NewGuid();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		await bookingService.CreateBookingAsync(eventId, userId);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
			bookingService.CreateBookingAsync(eventId, userId));
	}

	[Fact]
	public async Task CreateBooking_ForNonExistingEvent_ShouldReturnFalse()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingRepo = new EfBookingRepository(ctx);
		var eventRepo = new EfEventRepository(ctx);

		var bookingService = new BookingService(bookingRepo, eventRepo);

		var result = await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			0);
		var userId = Guid.NewGuid();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() =>
			bookingService.CreateBookingAsync(created.Value.ID, userId));
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			3);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);

		var bookingResult = await bookingService.CreateBookingAsync(created.Value.ID, user.Id);
		Assert.True(bookingResult.IsSuccess);

		var booking = await bookingRepo.GetBookingAsync(bookingResult.Value.Id);
		Assert.NotNull(booking);

		booking.Confirm();
		await bookingRepo.SaveChangesAsync();

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			1);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var bookingResult = await bookingService.CreateBookingAsync(eventId, user.Id);
		Assert.True(bookingResult.IsSuccess);

		var booking = await bookingRepo.GetBookingAsync(bookingResult.Value.Id);
		Assert.NotNull(booking);
		booking.Reject();
		await bookingRepo.SaveChangesAsync();

		var storedEvent = await eventRepo.GetByIdAsync(eventId);
		Assert.NotNull(storedEvent);
		storedEvent.ReleaseSeats();
		await eventRepo.SaveChangesAsync();

		var after = await eventService.GetEventById(eventId);
		Assert.True(after.IsSuccess);
		Assert.Equal(1, after.Value.AvailableSeats);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			1);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var booking1Result = await bookingService.CreateBookingAsync(eventId, user.Id);
		Assert.True(booking1Result.IsSuccess);

		var booking1 = await bookingRepo.GetBookingAsync(booking1Result.Value.Id);
		Assert.NotNull(booking1);
		booking1.Reject();
		await bookingRepo.SaveChangesAsync();

		var storedEvent = await eventRepo.GetByIdAsync(eventId);
		Assert.NotNull(storedEvent);
		storedEvent.ReleaseSeats();
		await eventRepo.SaveChangesAsync();

		var result2 = await bookingService.CreateBookingAsync(eventId, user.Id);

		Assert.True(result2.IsSuccess);
		Assert.NotNull(result2.Value);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			totalSeats);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var successes = 0;
		var exceptions = 0;

		var tasks = Enumerable.Range(0, 20).Select(async _ =>
		{
			try
			{
				var result = await bookingService.CreateBookingAsync(eventId, user.Id);
				if (result.IsSuccess) Interlocked.Increment(ref successes);
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

		var evt = new Event("title",
			"desc",
			DateTime.UtcNow.AddMinutes(1),
			DateTime.UtcNow.AddMinutes(2),
			totalSeats);

		var user = new User("username", "login", UserRole.User);
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();


		var created = await eventService.CreateEventAsync(ToDto(evt));
		Assert.True(created.IsSuccess);
		var eventId = created.Value.ID;

		var ids = new ConcurrentBag<Guid>();

		var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
		{
			var result = await bookingService.CreateBookingAsync(eventId, user.Id);
			Assert.True(result.IsSuccess);
			ids.Add(result.Value!.Id);
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, ids.Count);
		Assert.Equal(totalSeats, ids.Distinct().Count());
	}
}