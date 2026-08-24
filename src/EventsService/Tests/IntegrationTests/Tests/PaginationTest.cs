using Application.Services.EventService;
using Application.Services.EventService.Dto;
using Application.Services.Filters;
using Common.Tests.Interfaces;
using Infrastructure.Contexts;
using Infrastructure.Repositories;
using IntegrationTest.Tests.Fixture;

namespace IntegrationTest.Tests;

[Collection("Database")]
public sealed class PaginationTest : ABaseTestRepository<AppDbContext>, IClassFixture<PostgresContainerFixture>
{
	protected override string[] TablesToTruncate =>
		["events", "bookings"];

	public PaginationTest(PostgresContainerFixture fixture) : base(fixture, options => new AppDbContext(options))
	{
	}

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
				var dto = new EventDto
				{
					Title = $"title {i}",
					Description = "desc",
					StartAt = DateTime.UtcNow,
					EndAt = DateTime.UtcNow.AddMinutes(1),
					TotalSeats = 10
				};

				var created = await eventService.CreateEventAsync(dto);
				Assert.True(created.IsSuccess);
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