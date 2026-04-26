using System.Reflection;
using TestProject.Fixture;
using yandex_pract.CustomEventService;
using yandex_pract.CustomEventService.Models;

namespace EventTests.Tests;

[Collection("ShareDBCollection")]
[TestCaseOrderer("EventTests.Tests.PriorityOrder", "EventTests")]

public class CrudTests
{
	private readonly EventService _service;

	public CrudTests(TestDBFixture fixture)
	{
		_service = fixture.Service;
		Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name);
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
		//TODO Валидация в контроллере 
	}

	[Fact]
	public void GetAllEventsTest()
	{
		var requested = _service.GetEvents();
		Assert.NotNull(requested);
		Assert.True(requested.Count > 0);
	}

	[Fact, TestPriority(1)]
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
		Assert.False(evt.hasElement);
		Assert.Null(evt.resultModel);
	}

	[Fact, TestPriority(2)]
	public void UpdateEventTest()
	{
		var id = Guid.Parse("24c2f1d5-582e-4ccd-b60c-e0a00eae0588");
		var testTitle = "testTitle";
		var testDescription = "testDescription";
		var startAt = DateTime.Now;
		var endAt = DateTime.Now + TimeSpan.FromSeconds(15);

		var newTestEvent = new Event(testTitle, testDescription, startAt, endAt);
		
		var result = _service.TryUpdateEvent(id,newTestEvent);

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
		var id = Guid.Parse("24c2f1d5-582e-4ccd-1111-e0a00eae0000");
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

	[Fact, TestPriority(3)]
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