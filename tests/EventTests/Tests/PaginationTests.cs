using TestProject.Fixture;
using yandex_pract.CustomEventService;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class PaginationTests
{
	private readonly EventService _service;

	public PaginationTests(TestDBFixture fixture)
	{
		_service = fixture.EventService;
	}

	[Fact]
	public void PaginationTest()
	{
		var page1 = _service.GetEvents(null, null, null, 1, 5);
		var page2 = _service.GetEvents(null, null, null, 2, 5);

		Assert.Equal(5, page1.Data.Count);
		Assert.Equal(5, page2.Data.Count);
		Assert.NotEqual(page1.Data.First().ID, page2.Data.First().ID);
	}

}