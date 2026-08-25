using Common.Tests.Interfaces;
using EventsService.Domain.Models.Events;
using EventsService.Infrastructure.Contexts;
using EventsService.Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;

using Microsoft.EntityFrameworkCore;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class EventsRepositoryTest: ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["events"];
	
	public EventsRepositoryTest(PostgresContainerFixture fixture) : base(fixture, options => new AppDbContext(options))
	{
	}

	[Fact]
	public async Task CreateEvent_ShouldPersistToDatabase()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var evt = new Event(
			"title",
			"desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddHours(1),
			10);

		var repo = new EfEventRepository(context);
		
		await repo.AddAsync(evt);
		await repo.SaveChangesAsync();

		await using var verify = CreateContext();
		var saved = await verify.Events.FirstOrDefaultAsync(e => e.Id == evt.Id);

		Assert.NotNull(saved);
		Assert.Equal("title", saved.Title);
		Assert.Equal("desc", saved.Description);
		Assert.Equal(10, saved.TotalSeats);
		Assert.Equal(10, saved.AvailableSeats);
	}

	[Fact]
	public async Task GetEventById_ShouldReturnCorrectEntity()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var evt = new Event(
			"title",
			"desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddHours(1),
			10);

		context.Events.Add(evt);
		await context.SaveChangesAsync();

		var repo = new EfEventRepository(context);
		var loaded = await repo.GetByIdAsync(evt.Id);

		Assert.NotNull(loaded);
		Assert.Equal(evt.Title, loaded.Title);
		Assert.Equal(evt.Description, loaded.Description);
		Assert.Equal(evt.StartAt, loaded.StartAt);
		Assert.Equal(evt.EndAt, loaded.EndAt);
	}

	[Fact]
	public async Task UpdateEvent_ShouldModifyPersistedEntity()
	{
		await ResetDatabaseAsync();
		await using (var arrangeContext = CreateContext())
		{
			var evt = new Event(
				"oldTitle",
				"oldDesc",
				DateTime.UtcNow,
				DateTime.UtcNow.AddHours(1),
				10);

			arrangeContext.Events.Add(evt);
			var saved = await arrangeContext.SaveChangesAsync() > 0;
			Assert.True(saved);
		}

		await using (var updateContext = CreateContext())
		{
			var repo = new EfEventRepository(updateContext);

			var existing = await updateContext.Events.FirstAsync();

			var updated = new Event(
				"newTitle",
				"newDesc",
				existing.StartAt.AddHours(1),
				existing.EndAt.AddHours(1),
				20);
			existing.UpdateEvent(updated);
			await repo.SaveChangesAsync();
		}

		await using (var verifyContext = CreateContext())
		{
			var saved = await verifyContext.Events.FirstAsync();

			Assert.Equal("newTitle", saved.Title);
			Assert.Equal("newDesc", saved.Description);
			Assert.Equal(20, saved.TotalSeats);
			Assert.Equal(20, saved.AvailableSeats);
		}
	}

	[Fact]
	public async Task DeleteEvent_ShouldRemoveEntity()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var evt = new Event(
			"title",
			"desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddHours(1),
			10);

		context.Events.Add(evt);
		await context.SaveChangesAsync();

		var repo = new EfEventRepository(context);
		await repo.RemoveAsync(evt);
		await repo.SaveChangesAsync();

		await using var verify = CreateContext();
		Assert.False(await verify.Events.AnyAsync(e => e.Id == evt.Id));
	}

	[Fact]
	public async Task GetAllEvents_ShouldReturnAllPersistedEntities()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var evt1 = new Event("t1", "d1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);
		var evt2 = new Event("t2", "d2", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 20);

		context.Events.AddRange(evt1, evt2);
		await context.SaveChangesAsync();

		var repo = new EfEventRepository(context);
		var all = await repo.GetAllEventsAsync();

		Assert.Equal(2, all.Count);
		Assert.Contains(all, e => e.Title == "t1");
		Assert.Contains(all, e => e.Title == "t2");
	}

	[Fact]
	public async Task CreateEvent_ShouldFail_WhenEndAtLessThanStartAt()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var evt = new Event(
			"title",
			"desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddHours(-1),
			10);

		var repo = new EfEventRepository(context);

		await repo.AddAsync(evt);

		await Assert.ThrowsAsync<DbUpdateException>(() => repo.SaveChangesAsync());
	}

	[Fact]
	public async Task CreateEvent_ShouldFail_WhenAvailableSeatsGreaterThanTotalSeats()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var evt = new Event(
			"title",
			"desc",
			DateTime.UtcNow,
			DateTime.UtcNow.AddHours(1),
			10);

		context.Events.Add(evt);
		await context.SaveChangesAsync();

		evt.ReleaseSeats(100);

		var repo = new EfEventRepository(context);
		await Assert.ThrowsAsync<DbUpdateException>(() => repo.SaveChangesAsync());
	}

	[Fact]
	public async Task CreateEvent_ShouldAllowDuplicateTitleAndStartAt()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();

		var start = DateTime.UtcNow;

		var evt1 = new Event("title", "desc1", start, start.AddHours(1), 10);
		var evt2 = new Event("title", "desc2", start, start.AddHours(2), 20);

		var repo = new EfEventRepository(context);

		await repo.AddAsync(evt1);
		await repo.AddAsync(evt2);
		await repo.SaveChangesAsync();

		await using var verify = CreateContext();
		var events = await verify.Events.Where(e => e.Title == "title").ToListAsync();

		Assert.Equal(2, events.Count);
	}
}