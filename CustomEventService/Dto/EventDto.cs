using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.CustomEventService.Dto;

[Serializable]
public class EventDto : IValidatableObject
{
	public Guid ID { get; private set; }
	public string Title { get; set; }
	
	public string Description { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }

	[JsonConstructor]
	public EventDto()
	{
		
	}
	
	public EventDto(Event model)
	{
		ID = model.Id;
		Title = model.Title;
		Description = model.Description;
		StartTime = model.StartTime;
		EndTime = model.EndTime;
	}

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (string.IsNullOrEmpty(Title))
		{
			yield return new ValidationResult("Title could not be empty");
		}
		else if (Title.Length > 30 || Title.Length < 2)
		{
			yield return new ValidationResult("Title could not be smaller than 2 characters and bigger than 30");
		}

		if (EndTime <= StartTime)
		{
			yield return new ValidationResult("End date must be after start date.", new[] {nameof(EndTime)});
		}
	}
}