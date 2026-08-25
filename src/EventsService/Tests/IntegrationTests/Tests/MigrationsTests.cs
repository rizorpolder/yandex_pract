using Common.Tests.Interfaces;
using EventsService.Application.Services.Abstraction.Caching;
using EventsService.Application.Services.EventService;
using EventsService.Application.Services.EventService.Dto;
using EventsService.Application.Services.Filters;
using EventsService.Application.Services.Options;
using EventsService.Domain.Models.Events;
using EventsService.Infrastructure.Contexts;
using EventsService.Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace IntegrationTest.Tests;

[Collection("Database")]
public class MigrationsTests : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["events", "processed_messages"];

	public MigrationsTests(PostgresContainerFixture fixture) : base(fixture, options => new AppDbContext(options))
	{
	}

	[Fact]
	public async Task Migrations_ShouldApplySuccessfully()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var applied = await ctx.Database.GetAppliedMigrationsAsync();
		Assert.NotEmpty(applied);

		var pending = await ctx.Database.GetPendingMigrationsAsync();
		Assert.Empty(pending);
	}

	[Fact]
	public async Task Migrations_ShouldCreateEventsAndProcessedMessagesTables()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var eventsExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_name = 'events'")
			.FirstOrDefaultAsync();
		Assert.Equal(1, eventsExists);

		var processedExists = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_name = 'processed_messages'")
			.FirstOrDefaultAsync();
		Assert.Equal(1, processedExists);
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
				"WHERE table_name = 'events' AND column_name = 'id' AND data_type = 'uuid'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, idType);
	}

	[Fact]
	public async Task Migrations_ShouldAllowDecreaseSeatsOperation()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var eventRepo = new EfEventRepository(ctx);
		var filter = new EventFilterService();
		var cache = new Mock<ICacheService>();
		var cacheOptions = Options.Create(new CacheOptions
		{
			EventTtlSeconds = 300,
			TopEventsTtlSeconds = 300
		});

		var eventService = new EventService(eventRepo, filter, cache.Object, cacheOptions);


		var dto = new EventDto
		{
			Title = "title",
			Description = "desc",
			StartAt = DateTime.UtcNow.AddMinutes(5),
			EndAt = DateTime.UtcNow.AddMinutes(10),
			TotalSeats = 5
		};

		var created = await eventService.CreateEventAsync(dto);
		Assert.True(created.IsSuccess);

		var result = await eventService.DecreaseAvailableSeatsAsync(created.Value.ID, seatsCount: 3);
		Assert.True(result.IsSuccess);

		await using var verify = CreateContext();
		var saved = await verify.Events.FirstAsync(e => e.Id == created.Value.ID);
		Assert.Equal(2, saved.AvailableSeats);
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