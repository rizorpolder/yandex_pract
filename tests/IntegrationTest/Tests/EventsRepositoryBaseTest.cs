using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class EventsRepositoryBaseTest : BaseTestRepository
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

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

		var result = await repo.TryAddEventAsync(evt);
		Assert.True(result);

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
		var (haveData, loaded) = await repo.GetEventByIdAsync(evt.Id);

		Assert.True(haveData);
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
			await arrangeContext.SaveChangesAsync();
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
		
			var (hasElement, result) = await repo.TryUpdateEventAsync(existing.Id, updated);
			Assert.True(hasElement);
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
		var deleted = await repo.TryRemoveEventAsync(evt);

		Assert.True(deleted);

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

		await Assert.ThrowsAsync<DbUpdateException>(async () => { await repo.TryAddEventAsync(evt); });
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
		await Assert.ThrowsAsync<DbUpdateException>(async () => { await repo.TryUpdateEventAsync(evt.Id, evt); });
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

		Assert.True(await repo.TryAddEventAsync(evt1));
		Assert.True(await repo.TryAddEventAsync(evt2));

		await using var verify = CreateContext();
		var events = await verify.Events.Where(e => e.Title == "title").ToListAsync();

		Assert.Equal(2, events.Count);
	}
}