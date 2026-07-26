using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;
using yandex_pract.Filters;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class PaginationTest : BaseTestRepository
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

	[Fact]
	public async Task PaginationTest_ShouldReturnCorrectPages()
	{
		await ResetDatabaseAsync();

		await using (var arrange = CreateContext())
		{
			var eventRepo = new EfEventRepository(arrange);
			var filter = new EventFilterService();
			var eventService = new EventService(eventRepo, filter);

			for (int i = 0; i < 25; i++)
			{
				var evt = new Event(
					$"title {i}",
					"desc",
					DateTime.UtcNow,
					DateTime.UtcNow.AddMinutes(1),
					10);

				await eventService.CreateEventAsync(evt);
			}
		}

		await using (var act1 = CreateContext())
		{
			var eventRepo = new EfEventRepository(act1);
			var filter = new EventFilterService();
			var eventService = new EventService(eventRepo, filter);

			var page1 = await eventService.GetEvents(null, null, null, 1, 5);

			Assert.Equal(5, page1.Data.Count);
		}

		await using (var act2 = CreateContext())
		{
			var eventRepo = new EfEventRepository(act2);
			var filter = new EventFilterService();
			var eventService = new EventService(eventRepo, filter);

			var page2 = await eventService.GetEvents(null, null, null, 2, 5);

			Assert.Equal(5, page2.Data.Count);

			Assert.NotEqual(page2.Data.First().ID, page2.Data.Last().ID);
		}
	}
}