using System;

namespace yandex_pract.DbContext;

public class PriceHistory
{
	public int Id { get; set; }
	public int ProductId { get; set; }
	public DateTime Date { get; set; }
	public decimal Price { get; set; }
} 