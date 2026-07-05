using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace yandex_pract.DbContext.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder
			.HasOne(u => u.Profile)
			.WithOne()
			.HasForeignKey<UserProfile>(u => u.Id);
		PrepareData(builder);
	}

	private void PrepareData(
		EntityTypeBuilder<User> builder)
	{
		builder.HasData(
			new User { Id = 1, Name = "Alice" },
			new User { Id = 2, Name = "Bob" },
			new User { Id = 3, Name = "Charlie" }
		);
	}
}