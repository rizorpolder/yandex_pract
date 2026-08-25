using Common.Tests.Interfaces;
using BookingService.Domain.Models.BookingModel;
using BookingService.Infrastructure.Contexts;
using BookingService.Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookingService.IntegrationTests.Tests;

[Collection("Database")]
public class MigrationsTests : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["bookings"];

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
	public async Task Migrations_ShouldCreateBookingsTable()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var bookingsExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM information_schema.tables WHERE table_name = 'bookings'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, bookingsExists);
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
	public async Task Migrations_ShouldCreateIndexOnBookingsUserId()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var indexExists = await ctx.Database
			.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM pg_indexes WHERE indexname = 'ix_bookings_userid'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, indexExists);
	}

	[Fact]
	public async Task Migrations_ShouldCreateCorrectColumnTypes()
	{
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var idType = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.columns " +
				"WHERE table_name = 'bookings' AND column_name = 'id' AND data_type = 'uuid'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, idType);

		var eventIdType = await ctx.Database
			.SqlQueryRaw<int>(
				"SELECT 1 AS \"Value\" FROM information_schema.columns " +
				"WHERE table_name = 'bookings' AND column_name = 'event_id' AND data_type = 'uuid'")
			.FirstOrDefaultAsync();

		Assert.Equal(1, eventIdType);
	}

	[Fact]
	public async Task Migrations_ShouldAllowCreateBookingOperation()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();
		await ctx.Database.MigrateAsync();

		var repo = new EfBookingRepository(ctx);

		var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

		await repo.AddBookingAsync(booking);
		await repo.SaveChangesAsync();

		await using var verify = CreateContext();
		var saved = await verify.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);

		Assert.NotNull(saved);
		Assert.Equal(booking.EventId, saved.EventId);
		Assert.Equal(booking.UserId, saved.UserId);
	}
}