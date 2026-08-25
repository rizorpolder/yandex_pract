using BookingService.Application.Services.Abstraction.Broker;
using BookingService.Application.Services.Abstraction.Repositories;
using BookingService.Application.Services.BackgroundServices;
using BookingService.Domain.Exceptions;
using BookingService.Domain.Models.BookingModel;
using BookingService.Domain.Models.BookingModel.Options;
using Common.Models;
using Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BookingService.UnitTests.Tests;

public class BookingModelTests
{
	private (Mock<IBookingRepository> bookingRepo, Mock<IBookingConfirmedPublisher> publisher,
		Application.Services.Booking.BookingService bookingService) CreateServices(int limitPerUser = 10)
	{
		var bookingRepo = new Mock<IBookingRepository>();
		var publisher = new Mock<IBookingConfirmedPublisher>();
		var options = Options.Create(new BookingOptions {LimitPerUser = limitPerUser});

		var bookingService = new Application.Services.Booking.BookingService(bookingRepo.Object, options);

		return (bookingRepo, publisher, bookingService);
	}

	[Fact]
	public async Task CreateBookingAsync_ShouldCreatePendingBooking()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var eventId = Guid.NewGuid();
		var userId = Guid.NewGuid();

		bookingRepo.Setup(r => r.GetActiveBookingsCountAsync(userId)).ReturnsAsync(0);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<BookingModel>())).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await bookingService.CreateBookingAsync(eventId, userId);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(eventId, result.Value.EventId);
		Assert.Equal(userId, result.Value.UserId);
		Assert.Equal(BookingStatus.Pending, result.Value.Status);

		bookingRepo.Verify(r => r.AddBookingAsync(It.IsAny<BookingModel>()), Times.Once);
		bookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateBookingAsync_ShouldCreateUniqueIdsForSeveralBookings()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var eventId = Guid.NewGuid();
		var user1 = Guid.NewGuid();
		var user2 = Guid.NewGuid();

		bookingRepo.Setup(r => r.GetActiveBookingsCountAsync(It.IsAny<Guid>())).ReturnsAsync(0);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<BookingModel>())).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result1 = await bookingService.CreateBookingAsync(eventId, user1);
		var result2 = await bookingService.CreateBookingAsync(eventId, user2);

		Assert.True(result1.IsSuccess);
		Assert.True(result2.IsSuccess);
		Assert.NotEqual(result1.Value!.Id, result2.Value!.Id);

		bookingRepo.Verify(r => r.AddBookingAsync(It.IsAny<BookingModel>()), Times.Exactly(2));
	}

	[Fact]
	public async Task CreateBookingAsync_ShouldThrow_WhenLimitReached()
	{
		var (bookingRepo, _, bookingService) = CreateServices(limitPerUser: 10);

		var eventId = Guid.NewGuid();
		var userId = Guid.NewGuid();

		bookingRepo.Setup(r => r.GetActiveBookingsCountAsync(userId)).ReturnsAsync(10);

		await Assert.ThrowsAsync<BookingLimitReachedException>(() =>
			bookingService.CreateBookingAsync(eventId, userId));

		bookingRepo.Verify(r => r.AddBookingAsync(It.IsAny<BookingModel>()), Times.Never);
	}

	[Fact]
	public async Task CreateBookingAsync_ShouldSucceed_WhenBelowLimit()
	{
		var (bookingRepo, _, bookingService) = CreateServices(limitPerUser: 10);

		var eventId = Guid.NewGuid();
		var userId = Guid.NewGuid();

		bookingRepo.Setup(r => r.GetActiveBookingsCountAsync(userId)).ReturnsAsync(9);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<BookingModel>())).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await bookingService.CreateBookingAsync(eventId, userId);

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task CreateBookingAsync_LimitIsPerUser_NotGlobal()
	{
		var (bookingRepo, _, bookingService) = CreateServices(limitPerUser: 10);

		var eventId = Guid.NewGuid();
		var user1 = Guid.NewGuid();
		var user2 = Guid.NewGuid();

		bookingRepo.Setup(r => r.GetActiveBookingsCountAsync(user1)).ReturnsAsync(10);
		bookingRepo.Setup(r => r.GetActiveBookingsCountAsync(user2)).ReturnsAsync(0);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<BookingModel>())).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		await Assert.ThrowsAsync<BookingLimitReachedException>(() =>
			bookingService.CreateBookingAsync(eventId, user1));

		var result2 = await bookingService.CreateBookingAsync(eventId, user2);
		Assert.True(result2.IsSuccess);
	}

	[Fact]
	public async Task GetBookingByIdAsync_ShouldReturnBooking_WhenExists()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());
		bookingRepo.Setup(r => r.GetBookingAsync(booking.Id)).ReturnsAsync(booking);

		var result = await bookingService.GetBookingByIdAsync(booking.Id);

		Assert.True(result.IsSuccess);
		Assert.Equal(booking.Id, result.Value!.Id);
		Assert.Equal(booking.EventId, result.Value.EventId);
	}

	[Fact]
	public async Task GetBookingByIdAsync_ShouldFail_WhenNotFound()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var id = Guid.NewGuid();
		bookingRepo.Setup(r => r.GetBookingAsync(id)).ReturnsAsync((BookingModel?) null);

		var result = await bookingService.GetBookingByIdAsync(id);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public void Booking_Confirm_SetsStatusAndProcessedAt()
	{
		var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

		booking.Confirm();

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
	}

	[Fact]
	public void Booking_Reject_SetsStatusAndProcessedAt()
	{
		var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

		booking.Reject();

		Assert.Equal(BookingStatus.Rejected, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
	}

	[Fact]
	public void Booking_Cancel_SetsStatusToCancelled()
	{
		var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

		booking.Cancel();

		Assert.Equal(BookingStatus.Cancelled, booking.Status);
	}

	[Fact]
	public async Task CancelBookingAsync_ShouldSucceed_WhenOwner()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var userId = Guid.NewGuid();
		var booking = new BookingModel(Guid.NewGuid(), userId);

		bookingRepo.Setup(r => r.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
		bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await bookingService.CancelBookingAsync(booking.Id, userId, UserRole.User);

		Assert.True(result.IsSuccess);
		Assert.Equal(BookingStatus.Cancelled, booking.Status);
	}

	[Fact]
	public async Task CancelBookingAsync_ShouldSucceed_WhenAdmin_EvenIfNotOwner()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var ownerId = Guid.NewGuid();
		var adminId = Guid.NewGuid();
		var booking = new BookingModel(Guid.NewGuid(), ownerId);

		bookingRepo.Setup(r => r.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
		bookingRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await bookingService.CancelBookingAsync(booking.Id, adminId, UserRole.Admin);

		Assert.True(result.IsSuccess);
		Assert.Equal(BookingStatus.Cancelled, booking.Status);
	}

	[Fact]
	public async Task CancelBookingAsync_ShouldFail_WhenNotOwnerAndNotAdmin()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var ownerId = Guid.NewGuid();
		var otherId = Guid.NewGuid();
		var booking = new BookingModel(Guid.NewGuid(), ownerId);

		bookingRepo.Setup(r => r.GetBookingAsync(booking.Id)).ReturnsAsync(booking);

		var result = await bookingService.CancelBookingAsync(booking.Id, otherId, UserRole.User);

		Assert.False(result.IsSuccess);
		Assert.Equal("Forbidden", result.ErrorMessage);
		bookingRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
	}

	[Fact]
	public async Task CancelBookingAsync_ShouldFail_WhenBookingNotFound()
	{
		var (bookingRepo, _, bookingService) = CreateServices();

		var id = Guid.NewGuid();
		bookingRepo.Setup(r => r.GetBookingAsync(id)).ReturnsAsync((BookingModel?) null);

		var result = await bookingService.CancelBookingAsync(id, Guid.NewGuid(), UserRole.User);

		Assert.False(result.IsSuccess);
		Assert.Equal("NotFound", result.ErrorMessage);
	}

	[Fact]
	public async Task BackgroundBookingService_ShouldConfirmPendingBooking_AndPublishEvent()
	{
		var bookingRepo = new Mock<IBookingRepository>();
		var publisher = new Mock<IBookingConfirmedPublisher>();
		var logger = new Mock<ILogger<BackgroundBookingService>>();

		var booking = new BookingModel(Guid.NewGuid(), Guid.NewGuid());

		var tcs = new TaskCompletionSource();

		bookingRepo.Setup(r => r.GetPendingAsync())
			.ReturnsAsync(() => booking.Status == BookingStatus.Pending
				? new List<BookingModel> {booking}
				: new List<BookingModel>());

		bookingRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		publisher.Setup(p => p.PublishAsync(It.IsAny<BookingConfirmed>(), It.IsAny<CancellationToken>()))
			.Callback(() => tcs.TrySetResult())
			.Returns(Task.CompletedTask);

		var services = new ServiceCollection();
		services.AddSingleton(bookingRepo.Object);
		services.AddSingleton(publisher.Object);
		var provider = services.BuildServiceProvider();

		var worker = new BackgroundBookingService(provider.GetRequiredService<IServiceScopeFactory>(), logger.Object);

		using var cts = new CancellationTokenSource();
		var workerTask = worker.StartAsync(cts.Token);

		await tcs.Task;
		await cts.CancelAsync();
		await workerTask;

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
		publisher.Verify(p =>
				p.PublishAsync(It.Is<BookingConfirmed>(m => m.BookingId == booking.Id), It.IsAny<CancellationToken>()),
			Times.Once);
	}
}