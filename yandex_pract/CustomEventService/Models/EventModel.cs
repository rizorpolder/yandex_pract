namespace yandex_pract.CustomEventService.Models;

public class EventModel
{
	public string Description;
	public DateTime EndTime;
	public Guid ID;
	public DateTime StartTime;
	public string Title;

	public void UpdateEvent(EventModel eventModel)
	{
		Title = eventModel.Title;
		Description = eventModel.Description;
		StartTime = eventModel.StartTime;
		EndTime = eventModel.EndTime;
	}
}