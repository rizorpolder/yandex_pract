using TestProject.Tests.Database;
using yandex_pract.CustomEventService;

namespace TestProject.Fixture;

public class TestDBFixture
{
	public EventService Service { get; }
	
	private readonly TestDB _database = new TestDB();
	public TestDBFixture()
	{
		Service = new EventService(_database);
	}
}