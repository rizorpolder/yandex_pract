using Application.Services.EventService;
using Application.Services.EventService.Dto;
using Application.Services.Filters;
using Common.Tests.Interfaces;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class FilterTest : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

	public FilterTest(PostgresContainerFixture fixture) : base(fixture, options => new AppDbContext(options))
	{
	}

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

				var dto = new EventDto
				{
					Title = title,
					Description = "desc",
					StartAt = DateTime.UtcNow,
					EndAt = DateTime.UtcNow.AddMinutes(1),
					TotalSeats = 10
				};

				var created = await service.CreateEventAsync(dto);
				Assert.True(created.IsSuccess);
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
				var start = from.AddDays(i);
				var end = start.AddHours(1);

				var dto = new EventDto
				{
					Title = $"title {i}",
					Description = "desc",
					StartAt = start,
					EndAt = end,
					TotalSeats = 10
				};

				var created = await service.CreateEventAsync(dto);
				Assert.True(created.IsSuccess);
			}
		}

		await using (var act = CreateContext())
		{
			var repo = new EfEventRepository(act);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			var page = await service.GetEvents(null, from, to, 2, 5);

			Assert.Equal(5, page.Data.Count);
			Assert.All(page.Data,
				e =>
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
				var start = from.AddDays(i);
				var end = start.AddHours(1);

				var dto = new EventDto
				{
					Title = title,
					Description = "desc",
					StartAt = start,
					EndAt = end,
					TotalSeats = 10
				};

				var created = await service.CreateEventAsync(dto);
				Assert.True(created.IsSuccess);
			}
		}

		await using (var act = CreateContext())
		{
			var repo = new EfEventRepository(act);
			var filter = new EventFilterService();
			var service = new EventService(repo, filter);

			var page = await service.GetEvents("meeting", from, to, 1, 10);

			Assert.True(page.Data.Count <= 10);

			Assert.All(page.Data,
				e =>
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
				var dto = new EventDto
				{
					Title = $"title {i}",
					Description = "desc",
					StartAt = DateTime.UtcNow,
					EndAt = DateTime.UtcNow.AddMinutes(1),
					TotalSeats = 10
				};

				var created = await service.CreateEventAsync(dto);
				Assert.True(created.IsSuccess);
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