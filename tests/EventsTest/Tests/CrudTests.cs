using TestProject1.Fixture;
using yandex_pract.MockDB;

namespace TestProject1.Tests;

[Collection("ShareDBCollection")]
public class CrudTests
{
	private readonly MockDB _db;

	public CrudTests(TestDBFixture fixture)
	{
		_db = fixture.Db;
	}
	
	[Fact]
	public void CreateEventTest()
	{
	}

	[Fact]
	public void GetAllEventsTest()
	{
	}

	[Fact]
	public void GetEventByID()
	{
	}

	[Fact]
	public void UpdateEventTest()
	{
	}

	[Fact]
	public void DeleteEventTest()
	{
	}
}