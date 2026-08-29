using EventsService.Application.Services.Abstraction.Caching;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.EventService;
using EventsService.Application.Services.EventService.Dto;
using EventsService.Application.Services.Filters;
using EventsService.Application.Services.Options;
using EventsService.Domain.Models.Events;
using Microsoft.Extensions.Options;
using Moq;

namespace UnitTests.Tests.UnitTests;

public class EventServiceCacheTests
{
	private (Mock<IEventRepository> eventRepo, Mock<ICacheService> cache, EventService service) CreateServices(
		int eventTtl = 300,
		int topEventsTtl = 300)
	{
		var eventRepo = new Mock<IEventRepository>();
		var cache = new Mock<ICacheService>();
		var filter = new EventFilterService();
		var options = Options.Create(new CacheOptions
		{
			EventTtlSeconds = eventTtl,
			TopEventsTtlSeconds = topEventsTtl
		});

		var service = new EventService(eventRepo.Object, filter, cache.Object, options);

		return (eventRepo, cache, service);
	}
	
	[Fact]
	public async Task GetEventById_CacheHit_ShouldNotCallRepository()
	{
		var (eventRepo, cache, service) = CreateServices();

		var id = Guid.NewGuid();
		var cachedDto = new EventDto {ID = id, Title = "cached", TotalSeats = 10};

		cache.Setup(c => c.GetAsync<EventDto>($"event:{id}")).ReturnsAsync(cachedDto);

		var result = await service.GetEventById(id);

		Assert.True(result.IsSuccess);
		Assert.Equal("cached", result.Value!.Title);

		eventRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
		cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<EventDto>(), It.IsAny<TimeSpan>()), Times.Never);
	}

	[Fact]
	public async Task GetTopEvents_CacheHit_ShouldNotCallRepository()
	{
		var (eventRepo, cache, service) = CreateServices();

		var cachedList = new List<EventDto> {new() {Title = "top1"}};
		cache.Setup(c => c.GetAsync<List<EventDto>>("events:top10")).ReturnsAsync(cachedList);

		var result = await service.GetTopEventsAsync();

		Assert.Single(result);
		Assert.Equal("top1", result[0].Title);

		eventRepo.Verify(r => r.GetTopEventsAsync(It.IsAny<int>()), Times.Never);
		cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<EventDto>>(), It.IsAny<TimeSpan>()),
			Times.Never);
	}

	[Fact]
	public async Task GetEventById_CacheMiss_ShouldFetchFromRepositoryAndCacheResult()
	{
		var (eventRepo, cache, service) = CreateServices(eventTtl: 120);

		var evt = new Event("title", "desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);

		cache.Setup(c => c.GetAsync<EventDto>($"event:{evt.Id}")).ReturnsAsync((EventDto?) null);
		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

		var result = await service.GetEventById(evt.Id);

		Assert.True(result.IsSuccess);
		Assert.Equal(evt.Title, result.Value!.Title);

		eventRepo.Verify(r => r.GetByIdAsync(evt.Id), Times.Once);
		cache.Verify(c => c.SetAsync(
				$"event:{evt.Id}",
				It.Is<EventDto>(d => d.Title == evt.Title),
				TimeSpan.FromSeconds(120)),
			Times.Once);
	}

	[Fact]
	public async Task GetEventById_CacheMiss_EventNotFound_ShouldNotCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var id = Guid.NewGuid();
		cache.Setup(c => c.GetAsync<EventDto>($"event:{id}")).ReturnsAsync((EventDto?) null);
		eventRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event?) null);

		var result = await service.GetEventById(id);

		Assert.False(result.IsSuccess);
		cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<EventDto>(), It.IsAny<TimeSpan>()), Times.Never);
	}

	[Fact]
	public async Task GetTopEvents_CacheMiss_ShouldFetchFromRepositoryAndCacheResult()
	{
		var (eventRepo, cache, service) = CreateServices(topEventsTtl: 600);

		var events = new List<Event>
		{
			new("A", "d", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10),
			new("B", "d", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 5)
		};

		cache.Setup(c => c.GetAsync<List<EventDto>>("events:top10")).ReturnsAsync((List<EventDto>?) null);
		eventRepo.Setup(r => r.GetTopEventsAsync(10)).ReturnsAsync(events);

		var result = await service.GetTopEventsAsync();

		Assert.Equal(2, result.Count);
		eventRepo.Verify(r => r.GetTopEventsAsync(10), Times.Once);
		cache.Verify(c => c.SetAsync(
				"events:top10",
				It.Is<List<EventDto>>(l => l.Count == 2),
				TimeSpan.FromSeconds(600)),
			Times.Once);
	}

	[Fact]
	public async Task UpdateEventAsync_ShouldInvalidateEventCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var evt = new Event("old", "desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);
		var dto = new EventDto
		{
			ID = evt.Id, Title = "new", Description = "newDesc",
			StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(2), TotalSeats = 10
		};

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await service.UpdateEventAsync(evt.Id, dto);

		Assert.True(result.IsSuccess);
		cache.Verify(c => c.RemoveAsync($"event:{evt.Id}"), Times.Once);
	}

	[Fact]
	public async Task UpdateEventAsync_EventNotFound_ShouldNotTouchCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var id = Guid.NewGuid();
		var dto = new EventDto {ID = id, Title = "new"};

		eventRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event?) null);

		var result = await service.UpdateEventAsync(id, dto);

		Assert.False(result.IsSuccess);
		cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task RemoveEvent_ShouldInvalidateEventCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var evt = new Event("title", "desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);
		var dto = new EventDto {ID = evt.Id, Title = evt.Title};

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.RemoveAsync(evt)).Returns(Task.CompletedTask);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await service.RemoveEvent(dto);

		Assert.True(result.IsSuccess);
		cache.Verify(c => c.RemoveAsync($"event:{evt.Id}"), Times.Once);
	}

	[Fact]
	public async Task DecreaseAvailableSeatsAsync_ShouldInvalidateEventCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var evt = new Event("title", "desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await service.DecreaseAvailableSeatsAsync(evt.Id, 3);

		Assert.True(result.IsSuccess);
		Assert.Equal(7, evt.AvailableSeats);
		cache.Verify(c => c.RemoveAsync($"event:{evt.Id}"), Times.Once);
	}

	[Fact]
	public async Task DecreaseAvailableSeatsAsync_NoAvailableSeats_ShouldNotInvalidateCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var evt = new Event("title", "desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 2);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

		var result = await service.DecreaseAvailableSeatsAsync(evt.Id, 5);

		Assert.False(result.IsSuccess);
		cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
	}

	[Fact]
	public async Task DecreaseAvailableSeatsAsync_EventNotFound_ShouldNotTouchCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var id = Guid.NewGuid();
		eventRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event?) null);

		var result = await service.DecreaseAvailableSeatsAsync(id, 1);

		Assert.False(result.IsSuccess);
		cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task CreateEventAsync_ShouldNotTouchTopEventsCache()
	{
		var (eventRepo, cache, service) = CreateServices();

		var dto = new EventDto
		{
			Title = "title", Description = "desc",
			StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddHours(1), TotalSeats = 10
		};

		eventRepo.Setup(r => r.AddAsync(It.IsAny<Event>())).Returns(Task.CompletedTask);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await service.CreateEventAsync(dto);

		Assert.True(result.IsSuccess);

		cache.Verify(c => c.RemoveAsync("events:top10"), Times.Never);
		cache.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
	}
}