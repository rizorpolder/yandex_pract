using Common.Tests.Interfaces;
using EventsService.Infrastructure.Contexts;
using EventsService.Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class ProcessedMessageRepositoryTest : ABaseTestRepository<AppDbContext>,
	IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["processed_messages"];

	public ProcessedMessageRepositoryTest(PostgresContainerFixture fixture) : base(fixture,
		options => new AppDbContext(options))
	{
	}

	[Fact]
	public async Task IsProcessedAsync_ShouldReturnFalse_WhenNeverMarked()
	{
		await ResetDatabaseAsync();
		await using var context = CreateContext();
		await context.Database.MigrateAsync();

		var repo = new EfProcessedMessageRepository(context);

		var result = await repo.IsProcessedAsync(Guid.NewGuid());

		Assert.False(result);
	}

	[Fact]
	public async Task MarkAsProcessedAsync_ThenIsProcessedAsync_ShouldReturnTrue()
	{
		await ResetDatabaseAsync();
		var bookingId = Guid.NewGuid();

		await using (var context = CreateContext())
		{
			await context.Database.MigrateAsync();
			var repo = new EfProcessedMessageRepository(context);
			await repo.MarkAsProcessedAsync(bookingId);
		}

		await using var verifyContext = CreateContext();
		var verifyRepo = new EfProcessedMessageRepository(verifyContext);

		var result = await verifyRepo.IsProcessedAsync(bookingId);

		Assert.True(result);
	}

	[Fact]
	public async Task MarkAsProcessedAsync_ShouldPersistProcessedAtTimestamp()
	{
		await ResetDatabaseAsync();
		var bookingId = Guid.NewGuid();
		var before = DateTime.UtcNow;

		await using (var context = CreateContext())
		{
			await context.Database.MigrateAsync();
			var repo = new EfProcessedMessageRepository(context);
			await repo.MarkAsProcessedAsync(bookingId);
		}

		var after = DateTime.UtcNow;

		await using var verifyContext = CreateContext();
		var saved = await verifyContext.ProcessedMessages.FirstAsync(x => x.BookingId == bookingId);

		Assert.InRange(saved.ProcessedAt, before, after);
	}

	[Fact]
	public async Task MarkAsProcessedAsync_CalledTwiceWithSameBookingId_ShouldThrowDbUpdateException()
	{
		await ResetDatabaseAsync();
		var bookingId = Guid.NewGuid();

		await using (var context = CreateContext())
		{
			await context.Database.MigrateAsync();
			var repo = new EfProcessedMessageRepository(context);
			await repo.MarkAsProcessedAsync(bookingId);
		}

		await using var secondContext = CreateContext();
		var secondRepo = new EfProcessedMessageRepository(secondContext);

		await Assert.ThrowsAsync<DbUpdateException>(() => secondRepo.MarkAsProcessedAsync(bookingId));
	}

	[Fact]
	public async Task IsProcessedAsync_ShouldReturnFalse_ForDifferentBookingId()
	{
		await ResetDatabaseAsync();
		var markedId = Guid.NewGuid();
		var otherId = Guid.NewGuid();

		await using (var context = CreateContext())
		{
			await context.Database.MigrateAsync();
			var repo = new EfProcessedMessageRepository(context);
			await repo.MarkAsProcessedAsync(markedId);
		}

		await using var verifyContext = CreateContext();
		var verifyRepo = new EfProcessedMessageRepository(verifyContext);

		var result = await verifyRepo.IsProcessedAsync(otherId);

		Assert.False(result);
	}
}