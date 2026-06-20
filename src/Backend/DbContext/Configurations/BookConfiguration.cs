using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace yandex_pract.DbContext.Configurations;

//Задание параметров через конфигурацию (PK, FK и тп)

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
	public void Configure(EntityTypeBuilder<Book> builder)
	{
		builder.ToTable("books", "catalog");
		builder.HasKey(b => b.Id);

		builder.Property(b => b.Title)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(b => b.Isbn)
			.IsRequired()
			.HasMaxLength(17)
			.HasColumnName("isbn_code");

		builder.Property(b => b.Price)
			.HasColumnType("decimal(18,2)");

		builder.Property(b => b.CreatedAt)
			.HasDefaultValueSql("CURRENT_TIMESTAMP");

		builder.HasIndex(b => b.Isbn)
			.IsUnique();

		builder.Ignore(b => b.DisplayInfo);

		builder.HasOne(b => b.Author)
			.WithMany(a => a.Books)
			.HasForeignKey(b => b.AuthorId);
	}
}