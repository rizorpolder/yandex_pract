using System.ComponentModel.DataAnnotations;

namespace yandex_pract.CustomEventService.Dto;

public class EventModelDto : IValidatableObject
{
	public Guid ID = Guid.NewGuid();
	public string Title;
	public string Description;
	public DateTime StartTime;
	public DateTime EndTime;

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