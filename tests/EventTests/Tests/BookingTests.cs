using TestProject.Fixture;
using TestProject.Tests.Database;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.CustomException;
using yandex_pract.Services.BackgroundBookingService;
using yandex_pract.Services.BookingService;
using yandex_pract.Services.BookingService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
public class BookingTests
{
	private readonly IBookingService _service;
	private readonly IEventService _eventService;
	private readonly TestDB _database;

	public BookingTests(TestDBFixture fixture)
	{
		_service = fixture.BookingService;
		_eventService = fixture.EventService;
		_database = fixture.Database;
	}


	[Fact]
	public void CreateSingleBookingTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = _eventService.CreateEventAsync(evt);
		Assert.True(added);


		var bookingResult = _service.CreateBookingAsync(evt.Id).Result;
		Assert.True(bookingResult.result);
		Assert.NotNull(bookingResult.booking);
		Assert.Equal(evt.Id, bookingResult.booking.EventId);
	}

	[Fact]
	public void CreateSeveralBookingsTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = _eventService.CreateEventAsync(evt);
		Assert.True(added);

		var booking1 = _service.CreateBookingAsync(evt.Id).Result;
		var booking2 = _service.CreateBookingAsync(evt.Id).Result;

		Assert.True(booking1.result);
		Assert.True(booking2.result);
		Assert.NotNull(booking1.booking);
		Assert.NotNull(booking2.booking);
		Assert.Equal(evt.Id, booking1.booking.EventId);
		Assert.Equal(evt.Id, booking2.booking.EventId);
		Assert.NotEqual(booking1.booking.Id, booking2.booking.Id);
	}

	[Fact]
	public void GetBookingByIdTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var added = _eventService.CreateEventAsync(evt);
		Assert.True(added);
		var booking = _service.CreateBookingAsync(evt.Id).Result;
		Assert.True(booking.result);
		Assert.NotNull(booking.booking);

		var result = _service.GetBookingByIdAsync(booking.booking.Id).Result;
		Assert.True(result.haveBooking);
		Assert.NotNull(result.booking);
	}

	[Fact]
	public void CreateBookingWithWrongIdTest()
	{
		var booking = _service.CreateBookingAsync(Guid.NewGuid()).Result;
		Assert.False(booking.result);
		Assert.Null(booking.booking);
	}

	[Fact]
	public void CreateBookingForRemovedEventTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);

		_eventService.CreateEventAsync(evt);

		var removed = _eventService.RemoveEvent(evt);

		Assert.True(removed);

		var result = _service.CreateBookingAsync(evt.Id).Result;

		Assert.False(result.result);
		Assert.Null(result.booking);
	}

	[Fact]
	public void GetBookingWithBrokenIdTest()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddSeconds(10), 3);
		var booking = new Booking(evt.Id);
		var result = _service.GetBookingByIdAsync(booking.Id).Result;
		Assert.False(result.haveBooking);
		Assert.Null(result.booking);
	}

	[Fact]
	public async Task BookingStatusChangesAfterBackgroundProcessing()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		_eventService.CreateEventAsync(evt);

		var (result, booking) = await _service.CreateBookingAsync(evt.Id);
		Assert.True(result);
		Assert.Equal(BookingStatus.Pending, booking.Status);

		var worker = new BackgroundBookingService(_database, _database);

		booking.Confirm();
		_database.UpdateBooking(booking);

		var (found, updated) = await _service.GetBookingByIdAsync(booking.Id);

		Assert.True(found);
		Assert.Equal(BookingStatus.Confirmed, updated.Status);
	}

	[Fact]
	public void CreateBooking_DecreasesAvailableSeats()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		_eventService.CreateEventAsync(evt);

		var before = evt.AvailableSeats;

		var (result, booking) = _service.CreateBookingAsync(evt.Id).Result;

		Assert.True(result);
		Assert.NotNull(booking);

		var (found, updatedEvent) = _eventService.GetEventById(evt.Id);
		Assert.True(found);
		Assert.Equal(before - 1, updatedEvent.AvailableSeats);
	}

	[Fact]
	public void CreateSeveralBookings_UntilLimit_AllSuccessful()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 3);
		_eventService.CreateEventAsync(evt);

		var b1 = _service.CreateBookingAsync(evt.Id).Result.booking;
		var b2 = _service.CreateBookingAsync(evt.Id).Result.booking;
		var b3 = _service.CreateBookingAsync(evt.Id).Result.booking;

		Assert.NotNull(b1);
		Assert.NotNull(b2);
		Assert.NotNull(b3);

		Assert.NotEqual(b1.Id, b2.Id);
		Assert.NotEqual(b2.Id, b3.Id);
		Assert.NotEqual(b1.Id, b3.Id);

		var (_, updatedEvent) = _eventService.GetEventById(evt.Id);
		Assert.Equal(0, updatedEvent.AvailableSeats);
	}

	[Fact]
	public void CreateBooking_WhenSeatsExhausted_ThrowsNoAvailableSeatsException()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 1);
		_eventService.CreateEventAsync(evt);

		var first = _service.CreateBookingAsync(evt.Id).Result;
		Assert.True(first.result);

		Assert.Throws<NoAvailableSeatsException>(() => { _service.CreateBookingAsync(evt.Id).Wait(); });
	}


	[Fact]
	public void CreateBooking_ForNonExistingEvent_ReturnsFalse()
	{
		var id = Guid.NewGuid();

		var (result, booking) = _service.CreateBookingAsync(id).Result;

		Assert.False(result);
		Assert.Null(booking);
	}

	[Fact]
	public void CreateBooking_NoSeatsLeft_ThrowsNoAvailableSeatsException()
	{
		var evt = new Event("title", "desc", DateTime.Now, DateTime.Now.AddMinutes(1), 0);
		_eventService.CreateEventAsync(evt);

		Assert.Throws<NoAvailableSeatsException>(() => { _service.CreateBookingAsync(evt.Id).Wait(); });
	}
}