using yandex_pract.CustomEventService.Dto;

namespace yandex_pract.CustomEventService.Models;

[Serializable]
public class EventModel
{
	public Guid ID { get; set; } = Guid.NewGuid();
	public string Title { get; set; }
	public string Description{ get; set; }
	public DateTime StartTime{ get; set; }
	public DateTime EndTime{ get; set; }
	
	public EventModel(EventModelDto dto)
	{
		Title = dto.Title;
		Description = dto.Description;
		StartTime = dto.StartTime;
		EndTime = dto.EndTime;
	}

	public void UpdateEvent(EventModel eventModel)
	{
		Title = eventModel.Title;
		Description = eventModel.Description;
		StartTime = eventModel.StartTime;
		EndTime = eventModel.EndTime;
	}
}