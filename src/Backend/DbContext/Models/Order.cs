using System.Collections.Generic;

namespace yandex_pract.DbContext;

public class Order
{
	public int Id { get; set; }
	public string Number { get; set; }
	public int UserId { get; set; }
	public User User { get; set; }
	public List<Product> Products { get; set; }
	
	
	// параметр который помечается в бд при удалении без удаления объекта 
	public bool IsDeleted { get; set; }
} 