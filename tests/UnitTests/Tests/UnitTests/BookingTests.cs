using System.Collections.Concurrent;
using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.BackgroundBookingService;
using Application.Services.BookingService;
using Domain.Exceptions;
using Domain.Models.Booking;
using Domain.Models.Event;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.Filters;

namespace EventTests.Tests;

public class BookingTests
{
	private (Mock<IEventRepository> eventRepo,
		Mock<IBookingRepository> bookingRepo,
		IEventService eventService,
		IBookingService bookingService) CreateServices()
	{
		var eventRepo = new Mock<IEventRepository>();
		var bookingRepo = new Mock<IBookingRepository>();

		var filter = new EventFilterService();

		var eventService = new EventService(eventRepo.Object, filter);
		var bookingService = new BookingService(bookingRepo.Object, eventRepo.Object);

		return (eventRepo, bookingRepo, eventService, bookingService);
	}

	[Fact]
	public async Task CreateSingleBookingTest()
	{
		var (eventRepo, bookingRepo, eventService, bookingService) = CreateServices();

		var dto = new EventDto()
		{
			Title = "testTitle",
			Description = "testDescription",
			StartAt = DateTime.Now,
			EndAt = DateTime.Now.AddSeconds(10),
			TotalSeats = 3
		};

		var createdEvent = new Event(dto.Title, dto.Description, dto.StartAt, dto.EndAt, dto.TotalSeats);

		eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>()))
			.Returns(Task.CompletedTask);

		eventRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		eventRepo.Setup(r => r.GetByIdAsync(createdEvent.Id))
			.ReturnsAsync(createdEvent);

		var added = await eventService.CreateEventAsync(dto);
		Assert.True(added.IsSuccess);


		var booking = new Booking(createdEvent.Id);

		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>()))
			.Returns(Task.CompletedTask);

		bookingRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		bookingRepo.Setup(r => r.GetBookingAsync(booking.Id))
			.ReturnsAsync(booking);

		var bookingResult = await bookingService.CreateBookingAsync(createdEvent.Id);

		Assert.True(bookingResult.IsSuccess);
		Assert.NotNull(bookingResult.Value);
		Assert.Equal(createdEvent.Id, bookingResult.Value.EventId);

		eventRepo.Verify(r => r.AddAsync(It.IsAny<Event>()), Times.Once);
		bookingRepo.Verify(r => r.AddBookingAsync(It.IsAny<Booking>()), Times.Once);
	}


	[Fact]
	public async Task CreateSeveralBookingsTest()
	{
		var (eventRepo, bookingRepo, eventService, bookingService) = CreateServices();

		var dto = new EventDto()
		{
			Title = "title",
			Description = "desc",
			StartAt = DateTime.Now,
			EndAt = DateTime.Now.AddSeconds(10),
			TotalSeats = 3
		};

		var evt = new Event(dto.Title, dto.Description, dto.StartAt, dto.EndAt, dto.TotalSeats);

		eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>()))
			.Returns(Task.CompletedTask);

		eventRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id))
			.ReturnsAsync(evt);

		var added = await eventService.CreateEventAsync(dto);
		Assert.True(added.IsSuccess);

		var eventId = evt.Id;

		var booking1 = new Booking(eventId);
		var booking2 = new Booking(eventId);

		bookingRepo.SetupSequence(r => r.AddBookingAsync(It.IsAny<Booking>()))
			.Returns(Task.CompletedTask)
			.Returns(Task.CompletedTask);

		bookingRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		bookingRepo.Setup(r => r.GetBookingAsync(booking1.Id))
			.ReturnsAsync(booking1);

		bookingRepo.Setup(r => r.GetBookingAsync(booking2.Id))
			.ReturnsAsync(booking2);

		var result1 = await bookingService.CreateBookingAsync(eventId);
		var result2 = await bookingService.CreateBookingAsync(eventId);

		Assert.True(result1.IsSuccess);
		Assert.True(result2.IsSuccess);

		Assert.NotNull(result1.Value);
		Assert.NotNull(result2.Value);

		Assert.Equal(eventId, result1.Value.EventId);
		Assert.Equal(eventId, result2.Value.EventId);

		Assert.NotEqual(result1.Value.Id, result2.Value.Id);

		bookingRepo.Verify(r => r.AddBookingAsync(It.IsAny<Booking>()), Times.Exactly(2));
		bookingRepo.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
	}


	[Fact]
	public async Task GetBookingByIdTest()
	{
		var (_, bookingRepo, _, bookingService) = CreateServices();

		var booking = new Booking(Guid.NewGuid());

		bookingRepo.Setup(r => r.GetBookingAsync(booking.Id))
			.ReturnsAsync(booking);

		var result = await bookingService.GetBookingByIdAsync(booking.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(booking.Id, result.Value.Id);
		Assert.Equal(booking.EventId, result.Value.EventId);
	}

	[Fact]
	public async Task CreateBookingWithWrongIdTest()
	{
		var (eventRepo, _, _, bookingService) = CreateServices();

		var wrongId = Guid.NewGuid();

		eventRepo.Setup(r => r.GetByIdAsync(wrongId))
			.ReturnsAsync((Event?)null);

		var result = await bookingService.CreateBookingAsync(wrongId);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task CreateBookingForRemovedEventTest()
	{
		var (eventRepo, _, eventService, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);

		var dto = new EventDto()
		{
			ID = evt.Id,
			Title = evt.Title,
			Description = evt.Description,
			StartAt = evt.StartAt,
			EndAt = evt.EndAt,
			TotalSeats = evt.TotalSeats
		};

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id))
			.ReturnsAsync(evt);

		eventRepo.Setup(r => r.RemoveAsync(evt))
			.Returns(Task.CompletedTask);

		eventRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var removed = await eventService.RemoveEvent(dto);
		Assert.True(removed.IsSuccess);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id))
			.ReturnsAsync((Event?)null);

		var result = await bookingService.CreateBookingAsync(evt.Id);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task GetBookingWithBrokenIdTest()
	{
		var (_, bookingRepo, _, bookingService) = CreateServices();

		var brokenId = Guid.NewGuid();

		bookingRepo.Setup(r => r.GetBookingAsync(brokenId))
			.ReturnsAsync((Booking?)null);

		var result = await bookingService.GetBookingByIdAsync(brokenId);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task BookingStatusChangesAfterBackgroundProcessing()
	{
		var eventRepo = new Mock<IEventRepository>();
		var bookingRepo = new Mock<IBookingRepository>();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		var booking = new Booking(evt.Id);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id))
			.ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);
		
		bookingRepo.Setup(r => r.GetPendingAsync())
			.ReturnsAsync(() => booking.Status == BookingStatus.Pending
				? new List<Booking> { booking }
				: new List<Booking>());
		bookingRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var services = new ServiceCollection();
		services.AddSingleton(eventRepo.Object);
		services.AddSingleton(bookingRepo.Object);
		var provider = services.BuildServiceProvider();
		var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

		var worker = new BackgroundBookingService(scopeFactory);

		using var cts = new CancellationTokenSource();
		var workerStartTask = worker.StartAsync(cts.Token);

		await Task.Delay(200, CancellationToken.None);

		await cts.CancelAsync();
		await workerStartTask;

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
	}

	[Fact]
	public async Task CreateBooking_DecreasesAvailableSeats()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

		var before = evt.AvailableSeats;

		var result = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(before - 1, evt.AvailableSeats);
	}

	[Fact]
	public async Task CreateSeveralBookings_UntilLimit_AllSuccessful()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var totalSeats = 3;
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

		var b1 = await bookingService.CreateBookingAsync(evt.Id);
		var b2 = await bookingService.CreateBookingAsync(evt.Id);
		var b3 = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(b1.IsSuccess);
		Assert.True(b2.IsSuccess);
		Assert.True(b3.IsSuccess);

		Assert.NotNull(b1.Value);
		Assert.NotNull(b2.Value);
		Assert.NotNull(b3.Value);

		Assert.NotEqual(b1.Value.Id, b2.Value.Id);
		Assert.NotEqual(b2.Value.Id, b3.Value.Id);
		Assert.NotEqual(b1.Value.Id, b3.Value.Id);

		Assert.Equal(0, evt.AvailableSeats);
	}

	[Fact]
	public async Task CreateBooking_WhenSeatsExhausted_ThrowsNoAvailableSeatsException()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

		var first = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(first.IsSuccess);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(evt.Id));
	}

	[Fact]
	public async Task CreateBooking_ForNonExistingEvent_ReturnsFalse()
	{
		var (eventRepo, _, _, bookingService) = CreateServices();

		var id = Guid.NewGuid();

		eventRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event?)null);

		var result = await bookingService.CreateBookingAsync(id);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task CreateBooking_NoSeatsLeft_ThrowsNoAvailableSeatsException()
	{
		var (eventRepo, _, _, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 0);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

		await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(evt.Id));
	}

	[Fact]
	public void Booking_Confirm_SetsStatusAndProcessedAt()
	{
		var booking = new Booking(Guid.NewGuid());

		booking.Confirm();

		Assert.Equal(BookingStatus.Confirmed, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
	}

	[Fact]
	public void Booking_Reject_SetsStatusAndProcessedAt()
	{
		var booking = new Booking(Guid.NewGuid());

		booking.Reject();

		Assert.Equal(BookingStatus.Rejected, booking.Status);
		Assert.NotEqual(default, booking.ProcessedAt);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_RestoresAvailableSeats()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);

		Booking? captured = null;

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>()))
			.Callback<Booking>(b => captured = b)
			.Returns(Task.CompletedTask);

		var result = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(result.IsSuccess);
		Assert.NotNull(captured);

		Assert.Equal(0, evt.AvailableSeats);

		captured!.Reject();
		evt.ReleaseSeats();

		Assert.Equal(BookingStatus.Rejected, captured.Status);
		Assert.Equal(1, evt.AvailableSeats);
	}

	[Fact]
	public async Task Booking_Reject_ThenReleaseSeats_AllowsNewBooking()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);

		Booking? captured = null;

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>()))
			.Callback<Booking>(b => captured = b)
			.Returns(Task.CompletedTask);

		var result1 = await bookingService.CreateBookingAsync(evt.Id);
		Assert.True(result1.IsSuccess);
		Assert.NotNull(captured);

		captured!.Reject();
		evt.ReleaseSeats();

		var result2 = await bookingService.CreateBookingAsync(evt.Id);

		Assert.True(result2.IsSuccess);
		Assert.NotNull(result2.Value);
		Assert.Equal(evt.Id, result2.Value.EventId);
	}

	[Fact]
	public async Task ConcurrentBookings_NoOverbookingOccurs()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var totalSeats = 5;
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

		var exceptions = 0;
		var successes = 0;

		var tasks = Enumerable.Range(0, 20).Select(async _ =>
		{
			try
			{
				var result = await bookingService.CreateBookingAsync(evt.Id);
				if (result.IsSuccess)
					Interlocked.Increment(ref successes);
			}
			catch (NoAvailableSeatsException)
			{
				Interlocked.Increment(ref exceptions);
			}
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, successes);
		Assert.Equal(20 - totalSeats, exceptions);
		Assert.Equal(0, evt.AvailableSeats);
	}


	[Fact]
	public async Task ConcurrentBookings_AllIdsAreUnique()
	{
		var (eventRepo, bookingRepo, _, bookingService) = CreateServices();

		var totalSeats = 10;
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), totalSeats);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
		bookingRepo.Setup(r => r.AddBookingAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

		var ids = new ConcurrentBag<Guid>();

		var tasks = Enumerable.Range(0, totalSeats).Select(async _ =>
		{
			var result = await bookingService.CreateBookingAsync(evt.Id);

			Assert.True(result.IsSuccess);
			ids.Add(result.Value!.Id);
		});

		await Task.WhenAll(tasks);

		Assert.Equal(totalSeats, ids.Count);
		Assert.Equal(totalSeats, ids.Distinct().Count());
	}
}