using TestProject.Tests.Database;
using yandex_pract.CustomEventService;
using yandex_pract.Filters;

namespace TestProject.Fixture;

public class TestDBFixture
{
	public EventService Service { get; }

	private readonly TestDB _database = new TestDB();
	private readonly EventFilterService _eventFilterService = new EventFilterService();

	public TestDBFixture()
	{
		Service = new EventService(_database, _eventFilterService);
	}
}