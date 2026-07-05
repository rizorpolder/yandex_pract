using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace yandex_pract.DbContext.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
	public void Configure(EntityTypeBuilder<Order> builder)
	{
		builder
			.HasOne(o => o.User)
			.WithMany(o => o.Orders)
			.HasForeignKey(o => o.UserId);
		
		//токен конкурентности (проверяется при перезаписи,
		//чтоб не было перезатирание другим пользователем)
		builder.Property(o => o.Id).IsConcurrencyToken();
		// либо можно использовать стандартную колонку  postgresql
		builder.Property<uint>("Version")
			.IsRowVersion();
		
		
		builder
			.HasMany(o => o.Products)
			.WithMany(p => p.Orders)
			.UsingEntity(j => j.HasData(
				new { OrdersId = 1, ProductsId = 1 },
				new { OrdersId = 1, ProductsId = 2 },
				new { OrdersId = 3, ProductsId = 1 }
			));

		//глобальный фильтр который исключит сущности
		builder.HasQueryFilter(o=>!o.IsDeleted); 
		
		PrepareData(builder);
	}

	private void PrepareData(EntityTypeBuilder<Order> builder)
	{
		builder.HasData(
			new Order { Id = 1, Number = "ORD-001", UserId = 1 },
			new Order { Id = 2, Number = "ORD-002", UserId = 1 },
			new Order { Id = 3, Number = "SUMMER-SALE-2023", UserId = 2 },
			new Order { Id = 4, Number = "ORD-004", UserId = 2 },
			new Order { Id = 5, Number = "ORD-005", UserId = 1 },
			new Order { Id = 6, Number = "ORD-006", UserId = 1 },
			new Order { Id = 41, Number = "ORD-041", UserId = 2 },
			new Order { Id = 42, Number = "ORD-042", UserId = 2 },
			new Order { Id = 43, Number = "ORD-043", UserId = 2 },
			new Order { Id = 44, Number = "ORD-044", UserId = 2 },
			new Order { Id = 45, Number = "ORD-045", UserId = 2 }
		);
	}
}