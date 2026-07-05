using System.Collections.Generic;

namespace yandex_pract.DbContext;

public class Product
{
	public int Id { get; set; }
	public string Name { get; set; }
	public List<PriceHistory> PriceHistory { get; set; }
	public List<Order> Orders { get; set; }
	
	public Dictionary<string, string> Attributes { get; set; }

} 