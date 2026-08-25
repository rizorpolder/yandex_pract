using EventsService.Application.Services.Abstraction.Caching;
using EventsService.Application.Services.Abstraction.Repositories;
using EventsService.Application.Services.Abstraction.Services;
using EventsService.Application.Services.EventService;
using EventsService.Application.Services.Filters;
using EventsService.Application.Services.Options;
using EventsService.Domain.Models.Events;
using Microsoft.Extensions.Options;
using Moq;

namespace UnitTests.Tests.UnitTests;

public class PaginationTests
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
	public async Task PaginationTest()
	{
		var (eventRepo, eventService) = CreateServices();

		var events = new List<Event>();
		for (int i = 0; i < 25; i++)
		{
			events.Add(new Event(
				$"title {i}",
				"desc",
				DateTime.Now,
				DateTime.Now.AddMinutes(1),
				10));
		}

		eventRepo.Setup(r => r.GetAllEventsAsync())
			.ReturnsAsync(events);

		var page1 = await eventService.GetEvents(null, null, null, 1, 5);

		var page2 = await eventService.GetEvents(null, null, null, 2, 5);

		Assert.Equal(5, page1.Data.Count);
		Assert.Equal(5, page2.Data.Count);

		Assert.NotEqual(page1.Data.First().ID, page2.Data.First().ID);

		Assert.Equal("title 0", page1.Data.First().Title);
		Assert.Equal("title 5", page2.Data.First().Title);
	}
}