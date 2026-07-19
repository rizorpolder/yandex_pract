using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class PaginationTests
{
	private readonly EventService _service;

	public PaginationTests(TestDbFixture fixture)
	{
		_service = fixture.EventService;
	}

	[Fact]
	public async Task PaginationTest()
	{
		for (int i = 0; i < 25; i++)
		{
			var evt = new Event($"title {i}", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 10);
			await _service.CreateEventAsync(evt);
		}

		var page1 = await _service.GetEvents(null, null, null, 1, 5);
		var page2 = await _service.GetEvents(null, null, null, 2, 5);

		Assert.Equal(5, page1.Data.Count);
		Assert.Equal(5, page2.Data.Count);
		Assert.NotEqual(page1.Data.First().ID, page2.Data.First().ID);
	}
}