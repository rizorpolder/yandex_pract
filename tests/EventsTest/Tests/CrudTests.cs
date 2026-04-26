using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;
using yandex_pract.MockDB;

namespace TestProject.Tests;

[Collection("ShareDBCollection")]
public class CrudTests
{
	private readonly EventService _service;

	public CrudTests(TestDBFixture fixture)
	{
		_service = fixture.Service;
	}

	[Fact]
	public void CreateEventTest()
	{
		var testEvent = new Event("testTitle",
			"testDescription",
			DateTime.Now,
			DateTime.Now + TimeSpan.FromSeconds(15));

		var eventCallback = _service.AddEvent(testEvent);

		Assert.True(eventCallback);

		var requested = _service.GetEventById(testEvent.Id);
		Assert.True(requested.hasElement);
		Assert.NotNull(requested.resultModel);
		Assert.Equal(testEvent, requested.resultModel);
	}

	[Fact]
	public void CreateIncorrectEventTest()
	{
		var testEvent = new Event("testTitle",
			"testDescription",
			DateTime.Now,
			DateTime.Now);

		var eventCallback = _service.AddEvent(testEvent);

		Assert.False(eventCallback);

		var requested = _service.GetEventById(testEvent.Id);
		Assert.False(requested.hasElement);
		Assert.Null(requested.resultModel);
	}

	[Fact]
	public void GetAllEventsTest()
	{
		var requested = _service.GetEvents();
		Assert.NotNull(requested);
		Assert.True(requested.Count > 0);
	}

	[Fact]
	public void GetEventByID()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");

		var evt = _service.GetEventById(id);
		Assert.True(evt.hasElement);
		Assert.NotNull(evt.resultModel);
	}
	
	[Fact]
	public void GetEventByIncorrectID()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0000");

		var evt = _service.GetEventById(id);
		Assert.True(evt.hasElement);
		Assert.NotNull(evt.resultModel);
	}

	[Fact]
	public void UpdateEventTest()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");
		var testTitle = "testTitle";
		var testDescription = "testDescription";
		var startAt = DateTime.Now;
		var endAt = DateTime.Now + TimeSpan.FromSeconds(15);

		var result = _service.TryUpdateEvent(id, new Event(testTitle, testDescription, startAt, endAt));

		Assert.True(result.hasElement);
		Assert.NotNull(result.eventResult);
		Assert.Equal(testTitle, result.eventResult.Title);
		Assert.Equal(testDescription, result.eventResult.Description);
		Assert.Equal(startAt, result.eventResult.StartAt);
		Assert.Equal(endAt, result.eventResult.EndAt);
	}
	
	[Fact]
	public void UpdateBrokenIDEventTest()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0000");
		var testTitle = "testTitle";
		var testDescription = "testDescription";
		var startAt = DateTime.Now;
		var endAt = DateTime.Now + TimeSpan.FromSeconds(15);

		var result = _service.TryUpdateEvent(id, new Event(testTitle, testDescription, startAt, endAt));
		Assert.False(result.hasElement);
		Assert.Null(result.eventResult);
		
	}
	
	[Fact]
	public void UpdateIncorrectDateEventTest()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0000");
		var testTitle = "testTitle";
		var testDescription = "testDescription";
		
		var startAt = DateTime.Now + TimeSpan.FromSeconds(15);
		var endAt = DateTime.Now;

		var result = _service.TryUpdateEvent(id, new Event(testTitle, testDescription, startAt, endAt));
		Assert.False(result.hasElement);
		Assert.Null(result.eventResult);
		
	}

	[Fact]
	public void DeleteEventTest()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");
		var evt = _service.GetEventById(id);
		Assert.True(evt.hasElement);
		Assert.NotNull(evt.resultModel);

		var result = _service.RemoveEvent(evt.resultModel);
		Assert.True(result);
		var deleted = _service.GetEventById(id);
		Assert.False(deleted.hasElement);
		Assert.Null(deleted.resultModel);
	}
}