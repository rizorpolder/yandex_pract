using TestProject.Fixture;
using yandex_pract.CustomEventService;

namespace TestProject.Tests;

[Collection("ShareDBCollection")]
public class FilterTests
{
	private readonly EventService _service;

	public FilterTests(TestDBFixture fixture)
	{
		_service = fixture.Service;
	}

	[Fact]
	public void TitleFilterTest()
	{
	}
	
	[Fact]
	public void DateFilterTest()
	{
	}
}