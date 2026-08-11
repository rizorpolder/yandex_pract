using Application.Services.Abstraction.Repositories;
using Application.Services.Abstraction.Services;
using Application.Services.EventService;
using Application.Services.Filters;
using Domain.Models.Event;
using Moq;

namespace EventTests.Tests;

public class FilterTests
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
	public async Task TitleFilterTest()
	{
		var (eventRepo, eventService) = CreateServices();

		var events = new List<Event>
		{
			new("Meeting with team", "desc", DateTime.Now, DateTime.Now.AddHours(1), 10),
			new("Project meeting", "desc", DateTime.Now, DateTime.Now.AddHours(2), 5),
			new("Birthday party", "desc", DateTime.Now, DateTime.Now.AddHours(3), 20)
		};

		eventRepo.Setup(r => r.GetAllEventsAsync())
			.ReturnsAsync(events);

		var result = await eventService.GetEvents("meeting", null, null, 1, 10);

		Assert.All(result.Data,
			e => Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public async Task DateFilterTest()
	{
		var (eventRepo, eventService) = CreateServices();

		var events = new List<Event>
		{
			new("A", "desc", new DateTime(2024, 5, 10), new DateTime(2024, 5, 11), 10),
			new("B", "desc", new DateTime(2024, 7, 1), new DateTime(2024, 7, 2), 5),
			new("C", "desc", new DateTime(2023, 12, 1), new DateTime(2023, 12, 2), 20)
		};

		eventRepo.Setup(r => r.GetAllEventsAsync())
			.ReturnsAsync(events);

		var from = new DateTime(2024, 1, 1);
		var to = new DateTime(2024, 12, 31);

		var result = await eventService.GetEvents(null, from, to, 1, 10);

		Assert.All(result.Data,
			e =>
			{
				Assert.True(e.StartAt >= from);
				Assert.True(e.EndAt <= to);
			});
	}

	[Fact]
	public async Task CombinedFilterTest()
	{
		var (eventRepo, eventService) = CreateServices();

		var events = new List<Event>
		{
			new("Meeting with CEO", "desc", new DateTime(2024, 3, 10), new DateTime(2024, 3, 11), 10),
			new("Project meeting", "desc", new DateTime(2024, 6, 1), new DateTime(2024, 6, 2), 5),
			new("Random event", "desc", new DateTime(2024, 7, 1), new DateTime(2024, 7, 2), 20)
		};

		eventRepo.Setup(r => r.GetAllEventsAsync())
			.ReturnsAsync(events);

		var result = await eventService.GetEvents(
			"meeting",
			new DateTime(2024, 1, 1),
			new DateTime(2024, 12, 31),
			1,
			10);

		Assert.All(result.Data,
			e =>
			{
				Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase);
				Assert.True(e.StartAt >= new DateTime(2024, 1, 1));
				Assert.True(e.EndAt <= new DateTime(2024, 12, 31));
			});
	}
}