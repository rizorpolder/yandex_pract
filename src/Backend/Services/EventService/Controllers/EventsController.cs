using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yandex_pract.CustomEventService.Dto;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
	private readonly IEventService _eventService;

	public EventsController(IEventService eventService)
	{
		_eventService = eventService;
	}

	/// <summary>
	/// Получение всех эвентов
	/// </summary>
	[HttpGet(Name = nameof(GetEvents))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	public async Task<ActionResult<PaginatedResultDto>> GetEvents(string? title,
		DateTime? from,
		DateTime? to,
		int page = 1,
		int pageSize = 10)
	{
		var result = await _eventService.GetEvents(title, from, to, page, pageSize);
		return new OkObjectResult(result);
	}

	[HttpGet("{id:guid}", Name = nameof(GetEventById))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<EventDto>> GetEventById(Guid id)
	{
		var result = await _eventService.GetEventById(id);

		if (result.hasElement)
		{
			return new OkObjectResult(new EventDto(result.resultModel));
		}

		return NotFound();
	}

	[HttpPost(Name = nameof(CreateNewEvent))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<EventDto>> CreateNewEvent([FromBody] EventDto newEvent)
	{
		if (!TryValidateModel(newEvent))
			return BadRequest();

		var model = new Event(newEvent);
		var createResult = await _eventService.CreateEventAsync(model);
		if (createResult)
			return new OkObjectResult(new EventDto(model)) {StatusCode = StatusCodes.Status201Created};

		return BadRequest();
	}

	[HttpPut("{id:guid}",Name = nameof(UpdateEventById))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<EventDto>> UpdateEventById(Guid id, [FromBody] EventDto eventDto)
	{
		if (!TryValidateModel(eventDto))
			return BadRequest();

		var newModel = new Event(eventDto);
		var (hasElement, eventData) = await _eventService.GetEventById(id);
		if (!hasElement)
			return NotFound();

		eventData.UpdateEvent(newModel);
		var updateResult = await _eventService.TryUpdateEvent(eventData);
		if (!updateResult)
			return NotFound();

		return new OkObjectResult(new EventDto(eventData));
	}

	[HttpDelete("{id:guid}",Name = nameof(DeleteEventById))]
	[ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> DeleteEventById(Guid id)
	{
		var model = await _eventService.GetEventById(id);
		if (!model.hasElement)
			return NotFound();

		var isSuccess = await _eventService.RemoveEvent(model.resultModel);
		if (isSuccess)
			return Ok();

		return BadRequest();
	}
}