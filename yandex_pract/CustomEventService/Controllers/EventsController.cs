using Microsoft.AspNetCore.Mvc;

namespace yandex_pract.CustomEventService.Controllers;

[ApiController] [Route("api/events")]
public class EventsController : ControllerBase
{
	private readonly IEventService _eventService;

	public EventsController(IEventService eventService)
	{
		_eventService = eventService;
	}

	[HttpGet("api/events")]
	public IActionResult GetAddEvents()
	{
		return Ok();
	}

	[HttpGet("api/events/{id}")]
	public IActionResult GetEventById(int id)
	{
		return Ok();
	}

	[HttpPost("api/events")]
	public IActionResult CreateNewEvent()
	{
		//добавить валидацию
		return Ok();
	}

	[HttpPut("api/events/{id}")]
	public IActionResult UpdateEventById(int id)
	{
		//добавить валидацию
		return Ok();
	}

	[HttpDelete("api/events/{id}")]
	public IActionResult DeleteEventById(int id)
	{
		return Ok();
	}
}