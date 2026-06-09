using TestProject.Tests.Database;
using yandex_pract.CustomEventService;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace TestProject.Fixture;

public class TestDBFixture
{
	public EventService EventService { get; }
	public BookingService BookingService { get; }
	public TestDB Database { get; }

	private readonly TestDB _database = new TestDB();
	private readonly EventFilterService _eventFilterService = new EventFilterService();

	public TestDBFixture()
	{
		Database = new TestDB();
		EventService = new EventService(_database, _eventFilterService);
		BookingService = new BookingService(_database, _database);
	}
}