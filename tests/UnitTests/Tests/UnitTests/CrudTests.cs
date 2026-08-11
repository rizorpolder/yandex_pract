using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.BookingService;
using Application.Services.EventService;
using Application.Services.EventService.Dto;
using Application.Services.Filters;
using Domain.Models.Event;
using Moq;

namespace EventTests.Tests;

public class CrudTests
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
	public async Task CreateEventTest()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		var dto = new EventDto()
		{
			Title = "testTitle",
			Description = "testDescription",
			StartAt = DateTime.Now,
			EndAt = DateTime.Now.AddSeconds(10),
			TotalSeats = 3
		};


		eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>()))
			.Returns(Task.CompletedTask);

		eventRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var result = await eventService.CreateEventAsync(dto);

		Assert.True(result.IsSuccess);

		eventRepo.Verify(r => r.AddAsync(It.Is<Event>(e =>
			e.Title == dto.Title &&
			e.Description == dto.Description &&
			e.TotalSeats == dto.TotalSeats)), Times.Once);

		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
	}


	[Fact]
	public async Task GetAllEventsTest()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		var e1 = new Event("A", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		var e2 = new Event("B", "desc", DateTime.Now, DateTime.Now.AddMinutes(2), 5);

		eventRepo.Setup(r => r.GetAllEventsAsync())
			.ReturnsAsync(new List<Event> { e1, e2 });

		var result = await eventService.GetEvents(null, null, null, 1, 10);

		Assert.Equal(2, result.Data.Count);
		Assert.Contains(result.Data, e => e.Title == "A");
		Assert.Contains(result.Data, e => e.Title == "B");
	}

	[Fact]
	public async Task GetEventByID()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id))
			.ReturnsAsync(evt);

		var result = await eventService.GetEventById(evt.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(evt.Id, result.Value!.ID);
		Assert.Equal(evt.Title, result.Value.Title);
	}

	[Fact]
	public async Task GetEventByIncorrectID()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		var id = Guid.NewGuid();

		eventRepo.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync((Event?)null);

		var result = await eventService.GetEventById(id);

		Assert.False(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task UpdateEventTest()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		var original = new Event("oldTitle", "oldDescription",
			DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		var dto = new EventDto()
		{
			ID = original.Id,
			Title = "newTitle",
			Description = "newDescription",
			StartAt = DateTime.Now,
			EndAt = DateTime.Now.AddSeconds(20),
			TotalSeats = 3
		};

		eventRepo.Setup(r => r.GetByIdAsync(original.Id))
			.ReturnsAsync(original);

		eventRepo.Setup(r => r.SaveChangesAsync())
			.Returns(Task.CompletedTask);

		var result = await eventService.UpdateEventAsync(original.Id, dto);

		Assert.True(result.IsSuccess);
		Assert.Equal("newTitle", result.Value.Title);
		Assert.Equal("newDescription", result.Value.Description);

		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task UpdateBrokenIDEventTest()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		var dto = new EventDto()
		{
			ID = Guid.NewGuid(),
			Title = "newTitle",
			Description = "newDescription",
			StartAt = DateTime.Now,
			EndAt = DateTime.Now.AddSeconds(20),
			TotalSeats = 3
		};

		eventRepo.Setup(r => r.GetByIdAsync(dto.ID))
			.ReturnsAsync((Event?)null);

		var result = await eventService.UpdateEventAsync(dto.ID, dto);

		Assert.False(result.IsSuccess);
	}

	[Fact]
	public async Task DeleteEventTest()
	{
		var (eventRepo, _, eventService, _) = CreateServices();

		

		var evt = new Event("newTitle", "newDescription", DateTime.Now, DateTime.Now.AddSeconds(20), 3);

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
		Assert.NotNull(removed.Value);
		eventRepo.Verify(r => r.RemoveAsync(It.Is<Event>(e => e.Id == removed.Value.ID)), Times.Once);
		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
	}
}