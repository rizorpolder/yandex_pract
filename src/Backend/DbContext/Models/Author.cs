using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace yandex_pract.DbContext;

public class Author
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }

	public string Email { get; set; }
	public List<Book> Books { get; set; }
}