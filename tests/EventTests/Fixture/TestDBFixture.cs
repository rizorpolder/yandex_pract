using TestProject.Tests.Database;
using yandex_pract.CustomEventService;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace TestProject.Fixture;

public class TestDBFixture
{
	public EventService EventService { get; }
	public BookingService BookingService { get; }
	public TestDB Database => _database;
	
	private readonly TestDB _database = new TestDB();
	private readonly EventFilterService _eventFilterService = new EventFilterService();

	public TestDBFixture()
	{
		EventService = new EventService(_database, _eventFilterService);
		BookingService = new BookingService(_database, _database);
	}
}