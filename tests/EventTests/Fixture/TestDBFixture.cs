using Microsoft.EntityFrameworkCore;
using yandex_pract.CustomEventService;
using yandex_pract.DbContext;
using yandex_pract.DbContext.Interfaces;
using yandex_pract.Filters;
using yandex_pract.Services.BookingService;

namespace TestProject.Fixture;

public class TestDbFixture
{
	public AppDbContext DbContext { get; }
	public IEventDataBase EventDataBase { get; }
	public IBookingDataBase BookingDataBase { get; }

	public EventService EventService { get; }
	public BookingService BookingService { get; }

	public TestDbFixture()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
			.Options;

		DbContext = new AppDbContext(options);

		EventDataBase = new EfEventDataBase(DbContext);
		BookingDataBase = new EfBookingDataBase(DbContext);

		var filterService = new EventFilterService();

		EventService = new EventService(EventDataBase, filterService);
		BookingService = new BookingService(DbContext, EventDataBase);
	}
}