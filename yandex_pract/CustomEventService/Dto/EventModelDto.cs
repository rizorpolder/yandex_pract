using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace yandex_pract.CustomEventService.Dto;

[Serializable]
public class EventModelDto : IValidatableObject
{
	[JsonPropertyName("Title")]
	public string Title { get; set; }
	public string Description{ get; set; }
	public DateTime StartTime{ get; set; }
	public DateTime EndTime{ get; set; }

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