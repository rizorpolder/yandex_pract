using System;
using System.ComponentModel.DataAnnotations;

namespace yandex_pract.DbContext;

public class Book
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Isbn { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public DateTime CreatedAt { get; set; }
	public int AuthorId { get; set; }
	public Author Author { get; set; } = null!;
	public string DisplayInfo => $"{Title} ({Isbn})";
}