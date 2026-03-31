using System.ComponentModel.DataAnnotations;

namespace yandex_pract.Models;

public class Address
{
	public int Building { get; set; }
	public string Street { get; set; }
}

public class AddressDto
{
	[Range(0, 99, ErrorMessage = "Please enter a number between 0 and 99")]
	public int Building { get; set; }

	[Required(ErrorMessage = "Please enter a street name")]
	public string Street { get; set; }
}