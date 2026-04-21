using TestProject1.Fixture;
using yandex_pract.MockDB;

namespace TestProject1.Tests;

[Collection("ShareDBCollection")]
public class FilterTests
{
	private readonly MockDB _db;

	public FilterTests(TestDBFixture fixture)
	{
		_db = fixture.Db;
	}

	[Fact]
	public void TitleFilterTest()
	{
		
	}
}