using EventsService.Application.Services.Abstraction.Caching;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.EventService;
using EventsService.Application.Services.Filters;
using EventsService.Application.Services.Options;
using EventsService.Domain.Models.Events;
using Microsoft.Extensions.Options;
using Moq;

namespace UnitTests.Tests.UnitTests;

public class DecreaseSeatsTests
{
	private (Mock<IEventRepository> eventRepo,EventService service) CreateServices(int eventTtl = 300,
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

		return (eventRepo, service);
	}

	[Fact]
	public async Task DecreaseAvailableSeats_ShouldSucceed_WhenEnoughSeats()
	{
		var (eventRepo, eventService) = CreateServices();
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(30), totalSeats: 10);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await eventService.DecreaseAvailableSeatsAsync(evt.Id, seatsCount: 3);

		Assert.True(result.IsSuccess);
		Assert.Equal(7, evt.AvailableSeats);
		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DecreaseAvailableSeats_ShouldFail_WhenEventNotFound()
	{
		var (eventRepo, eventService) = CreateServices();
		var id = Guid.NewGuid();

		eventRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Event?) null);

		var result = await eventService.DecreaseAvailableSeatsAsync(id, seatsCount: 1);

		Assert.False(result.IsSuccess);
		Assert.Equal("NotFound", result.ErrorMessage);
		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
	}

	[Fact]
	public async Task DecreaseAvailableSeats_ShouldFail_WhenNotEnoughSeats()
	{
		var (eventRepo, eventService) = CreateServices();
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(30), totalSeats: 2);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);

		var result = await eventService.DecreaseAvailableSeatsAsync(evt.Id, seatsCount: 5);

		Assert.False(result.IsSuccess);
		Assert.Equal("NoAvailableSeats", result.ErrorMessage);
		Assert.Equal(2, evt.AvailableSeats);
		eventRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
	}

	[Fact]
	public async Task DecreaseAvailableSeats_ShouldSucceed_WhenExactSeatsMatch()
	{
		var (eventRepo, eventService) = CreateServices();
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(30), totalSeats: 5);

		eventRepo.Setup(r => r.GetByIdAsync(evt.Id)).ReturnsAsync(evt);
		eventRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await eventService.DecreaseAvailableSeatsAsync(evt.Id, seatsCount: 5);

		Assert.True(result.IsSuccess);
		Assert.Equal(0, evt.AvailableSeats);
	}
}