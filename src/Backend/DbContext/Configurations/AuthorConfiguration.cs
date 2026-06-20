using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace yandex_pract.DbContext.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
	public void Configure(EntityTypeBuilder<Author> builder)
	{
		builder.ToTable("author");
		builder.HasKey(b => b.Id);
		builder.Property(b => b.Email).IsRequired().HasMaxLength(255);
		builder.Property(b => b.FirstName).IsRequired().HasMaxLength(100);
		builder.Property(b => b.LastName).IsRequired().HasMaxLength(100);
	}
}