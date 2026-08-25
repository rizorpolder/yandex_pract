using BookingService.Application.Services.BackgroundServices;
using BookingService.Domain.Models.BookingModel;
using BookingService.Domain.Models.BookingModel.Options;
using BookingService.Infrastructure.Contexts;
using BookingService.Infrastructure.Repositories;
using Common.Models;
using Common.Tests.Interfaces;
using Contracts.Events;
using IntegrationTest.Tests.Fixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BookingService.IntegrationTests.Tests;

[Collection("Database")]
public sealed class BookingRepositoryTest : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["bookings"];

	public BookingRepositoryTest(PostgresContainerFixture fixture) : base(fixture, options => new AppDbContext(options))
	{
	}

	private static Application.Services.Booking.BookingService CreateService(
		AppDbContext ctx,
		int limitPerUser = 10)
	{
		var bookingRepo = new EfBookingRepository(ctx);
		var options = Options.Create(new BookingOptions {LimitPerUser = limitPerUser});
		return new Application.Services.Booking.BookingService(bookingRepo, options);
	}

	[Fact]
	public async Task CreateBooking_ShouldPersistToDatabase()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx);

		var eventId = Guid.NewGuid();
		var userId = Guid.NewGuid();

		var result = await bookingService.CreateBookingAsync(eventId, userId);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		await using var verify = CreateContext();
		var saved = await verify.Bookings.FirstOrDefaultAsync(b => b.Id == result.Value.Id);

		Assert.NotNull(saved);
		Assert.Equal(eventId, saved.EventId);
		Assert.Equal(userId, saved.UserId);
		Assert.Equal(BookingStatus.Pending, saved.Status);
	}

	[Fact]
	public async Task CreateSeveralBookings_ShouldCreateUniqueBookings()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx);

		var eventId = Guid.NewGuid();
		var user1 = Guid.NewGuid();
		var user2 = Guid.NewGuid();

		var b1 = await bookingService.CreateBookingAsync(eventId, user1);
		var b2 = await bookingService.CreateBookingAsync(eventId, user2);

		Assert.True(b1.IsSuccess);
		Assert.True(b2.IsSuccess);
		Assert.NotEqual(b1.Value!.Id, b2.Value!.Id);
	}

	[Fact]
	public async Task GetBookingById_ShouldReturnBooking()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx);

		var created = await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());
		Assert.True(created.IsSuccess);

		var loaded = await bookingService.GetBookingByIdAsync(created.Value!.Id);

		Assert.True(loaded.IsSuccess);
		Assert.NotNull(loaded.Value);
		Assert.Equal(created.Value.Id, loaded.Value.Id);
	}

	[Fact]
	public async Task GetBooking_WithBrokenId_ShouldReturnFailure()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx);

		var result = await bookingService.GetBookingByIdAsync(Guid.NewGuid());

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task CreateBooking_WhenUserReachedLimit_ShouldThrow()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx, limitPerUser: 1);

		var userId = Guid.NewGuid();

		var b1 = await bookingService.CreateBookingAsync(Guid.NewGuid(), userId);
		Assert.True(b1.IsSuccess);

		await Assert.ThrowsAsync<BookingService.Domain.Exceptions.BookingLimitReachedException>(() =>
			bookingService.CreateBookingAsync(Guid.NewGuid(), userId));
	}

	[Fact]
	public async Task CreateBooking_LimitIsPerUser_ShouldAllowOtherUser()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx, limitPerUser: 1);

		var eventId = Guid.NewGuid();
		var userA = Guid.NewGuid();
		var userB = Guid.NewGuid();

		var b1 = await bookingService.CreateBookingAsync(eventId, userA);
		Assert.True(b1.IsSuccess);

		var b2 = await bookingService.CreateBookingAsync(eventId, userB);

		Assert.True(b2.IsSuccess);
	}

	[Fact]
	public async Task CancelBooking_ShouldPersistCancelledStatus()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx);

		var userId = Guid.NewGuid();
		var created = await bookingService.CreateBookingAsync(Guid.NewGuid(), userId);
		Assert.True(created.IsSuccess);

		var cancelled = await bookingService.CancelBookingAsync(created.Value!.Id, userId, UserRole.User);
		Assert.True(cancelled.IsSuccess);

		await using var verify = CreateContext();
		var saved = await verify.Bookings.FirstAsync(b => b.Id == created.Value.Id);
		Assert.Equal(BookingStatus.Cancelled, saved.Status);
	}

	[Fact]
	public async Task CancelBooking_ByNonOwner_ShouldFail()
	{
		await ResetDatabaseAsync();
		await using var ctx = CreateContext();

		var bookingService = CreateService(ctx);

		var ownerId = Guid.NewGuid();
		var otherId = Guid.NewGuid();

		var created = await bookingService.CreateBookingAsync(Guid.NewGuid(), ownerId);
		Assert.True(created.IsSuccess);

		var result = await bookingService.CancelBookingAsync(created.Value!.Id, otherId, UserRole.User);

		Assert.False(result.IsSuccess);

		await using var verify = CreateContext();
		var saved = await verify.Bookings.FirstAsync(b => b.Id == created.Value.Id);
		Assert.Equal(BookingStatus.Pending, saved.Status);
	}

	[Fact]
	public async Task BackgroundBookingService_ShouldConfirmPendingBookings_AndPublishEvent()
	{
		await ResetDatabaseAsync();

		var services = new ServiceCollection();
		services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_fixture.Postgres.GetConnectionString()));
		services.AddScoped<Application.Services.Abstraction.Repositories.IBookingRepository, EfBookingRepository>();

		var published = new TaskCompletionSource();

		var publisherMock = new Mock<Application.Services.Abstraction.Broker.IBookingConfirmedPublisher>();
		publisherMock
			.Setup(p => p.PublishAsync(It.IsAny<BookingConfirmed>(), It.IsAny<CancellationToken>()))
			.Callback(() => published.TrySetResult())
			.Returns(Task.CompletedTask);

		services.AddSingleton(publisherMock.Object);

		services.Configure<BookingOptions>(o => o.LimitPerUser = 10);

		var provider = services.BuildServiceProvider();

		Guid bookingId;
		await using (var setupCtx = CreateContext())
		{
			var repo = new EfBookingRepository(setupCtx);
			var options = Options.Create(new BookingOptions {LimitPerUser = 10});
			var bookingService = new Application.Services.Booking.BookingService(repo, options);

			var created = await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());
			Assert.True(created.IsSuccess);
			bookingId = created.Value!.Id;
		}

		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
		var worker = new BackgroundBookingService(
			scopeFactory,
			NullLogger<BackgroundBookingService>.Instance);

		using var cts = new CancellationTokenSource();
		var workerTask = worker.StartAsync(cts.Token);

		await published.Task;
		await cts.CancelAsync();
		await workerTask;

		await using var verify = CreateContext();
		var saved = await verify.Bookings.FirstAsync(b => b.Id == bookingId);

		Assert.Equal(BookingStatus.Confirmed, saved.Status);
		Assert.NotEqual(default, saved.ProcessedAt);
	}
}