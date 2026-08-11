using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Services.EventService.Dto;

[Serializable]
public class EventDto : IValidatableObject
{
	public Guid ID { get; set; }
	public string Title { get; set; }

	public string Description { get; set; }
	public DateTime StartAt { get; set; }
	public DateTime EndAt { get; set; }
	
	public int TotalSeats { get; set; }
	public int AvailableSeats { get; set; }

	[JsonConstructor]
	public EventDto()
	{
	}

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (string.IsNullOrEmpty(Title))
		{
			yield return new ValidationResult("Title could not be empty");
		}
		else if (TotalSeats < 0)
		{
			yield return new ValidationResult("Should be greater than or equal to zero");
		}
		
		else if (Title.Length is > 30 or < 2)
		{
			yield return new ValidationResult("Title could not be smaller than 2 characters and bigger than 30");
		}

		if (EndAt <= StartAt)
		{
			yield return new ValidationResult("End date must be after start date.", [nameof(EndAt)]);
		}
	}
}