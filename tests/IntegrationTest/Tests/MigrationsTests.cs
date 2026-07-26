using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;
using yandex_pract.Services.BookingService.Models;

namespace IntegrationTest.Tests;

[Collection("Database")]
public class MigrationsTests : BaseTestRepository
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

	[Fact]
	public async Task Migrations_ShouldApplySuccessfully()
	{
		await using var ctx = CreateContext();

		var pending = await ctx.Database.GetPendingMigrationsAsync();
		Assert.NotEmpty(pending);

		await ctx.Database.MigrateAsync();

		var applied = await ctx.Database.GetAppliedMigrationsAsync();
		Assert.NotEmpty(applied);
	}

	[Fact]
	public async Task Migrations_ShouldCreateEventsAndBookingsTables()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var eventsExists = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM information_schema.tables WHERE table_name = 'events'");

		Assert.Equal(1, eventsExists);

		var bookingsExists = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM information_schema.tables WHERE table_name = 'bookings'");

		Assert.Equal(1, bookingsExists);
	}

	[Fact]
	public async Task Migrations_ShouldCreateCheckConstraints()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var seatsCheck = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM pg_constraint WHERE conname = 'ck_events_available_seats'");

		Assert.Equal(1, seatsCheck);

		var timeCheck = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM pg_constraint WHERE conname = 'ck_events_time_range'");

		Assert.Equal(1, timeCheck);
	}

	[Fact]
	public async Task Migrations_ShouldEnforceForeignKeyConstraint()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		ctx.Bookings.Add(new Booking(Guid.NewGuid()));

		await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
	}

	[Fact]
	public async Task Migrations_ShouldCreateIndexOnBookingsEventId()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var indexExists = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM pg_indexes WHERE indexname = 'ix_bookings_eventid'");

		Assert.Equal(1, indexExists);
	}

	[Fact]
	public async Task Migrations_ShouldCreateCorrectColumnTypes()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var startAtType = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM information_schema.columns " +
			"WHERE table_name = 'events' AND column_name = 'start_at' AND data_type = 'timestamp with time zone'");

		Assert.Equal(1, startAtType);

		var idType = await ctx.Database.ExecuteSqlRawAsync(
			"SELECT 1 FROM information_schema.columns " +
			"WHERE table_name = 'bookings' AND column_name = 'id' AND data_type = 'uuid'");

		Assert.Equal(1, idType);
	}

	[Fact]
	public async Task Migrations_ShouldAllowRepositoryOperations()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var eventRepo = new EfEventRepository(ctx);
		var bookingRepo = new EfBookingRepository(ctx);
		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo, filter);
		var bookingService = new BookingService(bookingRepo, eventRepo);

		var evt = new Event("title", "desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			5);

		Assert.True(await eventService.CreateEventAsync(evt));

		var (result, booking) = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result);
		Assert.NotNull(booking);
	}

	[Fact]
	public async Task Migrations_ShouldEnforceAvailableSeatsCheck()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var evt = new Event("t", "d",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			10);

		evt.ReleaseSeats(100); // available_seats = 110

		ctx.Events.Add(evt);

		await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
	}

	[Fact]
	public async Task Migrations_ShouldEnforceTimeRangeCheck()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var evt = new Event("t", "d",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(-1),
			10);

		ctx.Events.Add(evt);

		await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
	}
}