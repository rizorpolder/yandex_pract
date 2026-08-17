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
		if (string.IsNullOrWhiteSpace(Title))
			yield return new ValidationResult("Title could not be empty", [nameof(Title)]);

		if (Title is {Length: < 2 or > 30})
			yield return new ValidationResult("Title must be between 2 and 30 characters", [nameof(Title)]);

		if (TotalSeats < 0)
			yield return new ValidationResult("TotalSeats must be greater than or equal to zero", [nameof(TotalSeats)]);

		if (EndAt <= StartAt)
			yield return new ValidationResult("End date must be after start date.", [nameof(EndAt)]);
	}
}