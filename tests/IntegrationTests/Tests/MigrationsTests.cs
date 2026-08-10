using Application.Services.BookingService;
using Domain.Models.Booking;
using Domain.Models.Event;
using Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.Filters;

namespace IntegrationTest.Tests;

[Collection("Database")]
public class MigrationsTests(PostgresContainerFixture fixture) : ABaseTestRepository(fixture)
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

	[Fact]
	public async Task Migrations_ShouldApplySuccessfully()
	{
		await using var ctx = CreateContext();

		await ctx.Database.MigrateAsync();

		var applied = await ctx.Database.GetAppliedMigrationsAsync();
		Assert.NotEmpty(applied);
		Assert.Contains(applied, m => m.Contains("InitialCreate"));

		var pending = await ctx.Database.GetPendingMigrationsAsync();
		Assert.Empty(pending);
	}

	[Fact]
	public async Task Migrations_ShouldCreateEventsAndBookingsTables()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var eventsExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_name = 'events'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, eventsExists);

		var bookingsExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_name = 'bookings'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, bookingsExists);
	}

	[Fact]
	public async Task Migrations_ShouldCreateCheckConstraints()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var seatsCheck = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM pg_constraint WHERE conname = 'ck_events_available_seats'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, seatsCheck);

		var timeCheck = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM pg_constraint WHERE conname = 'ck_events_time_range'")
			.FirstOrDefaultAsync();

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

		var indexExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM pg_indexes WHERE indexname = 'ix_bookings_eventid'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, indexExists);
	}

	[Fact]
	public async Task Migrations_ShouldCreateCorrectColumnTypes()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var startAtType = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.columns " +
				"WHERE table_name = 'events' AND column_name = 'start_at' AND data_type = 'timestamp with time zone'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, startAtType);

		var idType = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.columns " +
				"WHERE table_name = 'bookings' AND column_name = 'id' AND data_type = 'uuid'")
			.FirstOrDefaultAsync();

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

		var dto = new EventDto
		{
			Title = "title",
			Description = "desc",
			StartAt = DateTime.UtcNow,
			EndAt = DateTime.UtcNow.AddMinutes(1),
			TotalSeats = 5
		};

		var created = await eventService.CreateEventAsync(dto);
		Assert.True(created.IsSuccess);

		var bookingResult = await bookingService.CreateBookingAsync(created.Value.ID);

		Assert.True(bookingResult.IsSuccess);
		Assert.NotNull(bookingResult.Value);
	}

	[Fact]
	public async Task Migrations_ShouldEnforceAvailableSeatsCheck()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var evt = new Event("t",
			"d",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(1),
			10);

		evt.ReleaseSeats(100); 

		ctx.Events.Add(evt);

		await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
	}

	[Fact]
	public async Task Migrations_ShouldEnforceTimeRangeCheck()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var evt = new Event("t",
			"d",
			DateTime.UtcNow,
			DateTime.UtcNow.AddMinutes(-1),
			10);

		ctx.Events.Add(evt);

		await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
	}
}