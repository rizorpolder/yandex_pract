using TestProject.Fixture;
using yandex_pract.CustomEventService;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class FilterTests
{
	private readonly EventService _service;

	public FilterTests(TestDBFixture fixture)
	{
		_service = fixture.EventService;
	}

	[Fact]
	public void TitleFilterTest()
	{
		var result = _service.GetEvents("meeting", null, null, 1, 10);

		Assert.All(result.Data,
			e =>
				Assert.Contains("meeting", e.Title, StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void DateFilterTest()
	{
		var from = new DateTime(2024, 1, 1);
		var to = new DateTime(2024, 12, 31);

		var result = _service.GetEvents(null, from, to, 1, 10);

		Assert.All(result.Data,
			e =>
			{
				Assert.True(e.StartAt >= from);
				Assert.True(e.EndAt <= to);
			});
	}

	[Fact]
	public void CombinedFilterTest()
	{
		var result = _service.GetEvents("meeting",
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