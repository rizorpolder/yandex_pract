using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.EventService;
using Application.Services.Filters;
using Domain.Models.Events;
using Moq;

namespace UnitTests.Tests.UnitTests;

public class PaginationTests
{
	private (Mock<IEventRepository> eventRepo,
		IEventService eventService) CreateServices()
	{
		var eventRepo = new Mock<IEventRepository>();
		var filter = new EventFilterService();
		var eventService = new EventService(eventRepo.Object, filter);

		return (eventRepo, eventService);
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