using yandex_pract.MockDB;

namespace TestProject1;

public class EventDBTest
{
	private ICustomDataBase db;

	public EventDBTest()
	{
		db = new MockDB();
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

	[Fact]
	public void FilterByTitleTest()
	{
	}

	[Fact]
	public void FilterByDateTest()
	{
	}
}