using IntegrationTest.Tests.Interfaces;
using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.DbContext;
using yandex_pract.Filters;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class FilterTest : ABaseTestRepository
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

	[Fact]
	public async Task PaginationWithTitleFilter_ShouldReturnCorrectFilteredPage()
	{
		await ResetDatabaseAsync();

		await using (var arrange = CreateContext())
		{
			var repo = new EfEventRepository(arrange);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			for (int i = 0; i < 30; i++)
			{
				var title = i % 2 == 0 ? $"meeting {i}" : $"other {i}";
				var evt = new Event(title, "desc", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), 10);
				await service.CreateEventAsync(evt);
			}
		}

		await using (var act = CreateContext())
		{
			var repo = new EfEventRepository(act);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			var page = await service.GetEvents("meeting", null, null, 1, 5);

			Assert.Equal(5, page.Data.Count);
			Assert.All(page.Data, e => Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase));
		}
	}

	[Fact]
	public async Task PaginationWithDateFilter_ShouldReturnCorrectFilteredPage()
	{
		await ResetDatabaseAsync();

		var from = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var to = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

		await using (var arrange = CreateContext())
		{
			var repo = new EfEventRepository(arrange);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			for (int i = 0; i < 30; i++)
			{
				var start = from.AddDays(i); // уже UTC
				var end = start.AddHours(1);

				var evt = new Event($"title {i}", "desc", start, end, 10);
				await service.CreateEventAsync(evt);
			}
		}

		await using (var act = CreateContext())
		{
			var repo = new EfEventRepository(act);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			var page = await service.GetEvents(null, from, to, 2, 5);

			Assert.Equal(5, page.Data.Count);
			Assert.All(page.Data, e =>
			{
				Assert.True(e.StartAt >= from);
				Assert.True(e.EndAt <= to);
			});
		}
	}


	[Fact]
	public async Task PaginationWithCombinedFilters_ShouldReturnCorrectFilteredPage()
	{
		await ResetDatabaseAsync();

		var from = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		var to = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);

		await using (var arrange = CreateContext())
		{
			var repo = new EfEventRepository(arrange);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			for (int i = 0; i < 40; i++)
			{
				var title = i % 3 == 0 ? $"meeting {i}" : $"other {i}";
				var start = from.AddDays(i); // UTC
				var end = start.AddHours(1);

				var evt = new Event(title, "desc", start, end, 10);
				await service.CreateEventAsync(evt);
			}
		}

		await using (var act = CreateContext())
		{
			var repo = new EfEventRepository(act);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			var page = await service.GetEvents("meeting", from, to, 1, 10);

			Assert.True(page.Data.Count <= 10);

			Assert.All(page.Data, e =>
			{
				Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase);
				Assert.True(e.StartAt >= from);
				Assert.True(e.EndAt <= to);
			});
		}
	}


	[Fact]
	public async Task PaginationOutOfRange_ShouldReturnEmptyPage()
	{
		await ResetDatabaseAsync();

		await using (var arrange = CreateContext())
		{
			var repo = new EfEventRepository(arrange);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			for (int i = 0; i < 10; i++)
			{
				var evt = new Event($"title {i}", "desc", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1), 10);
				await service.CreateEventAsync(evt);
			}
		}

		await using (var act = CreateContext())
		{
			var repo = new EfEventRepository(act);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			var page = await service.GetEvents(null, null, null, 5, 5);

			Assert.Empty(page.Data);
		}
	}
}