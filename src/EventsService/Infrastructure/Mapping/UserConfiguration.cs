using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("users");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
		builder.Property(x => x.Login).HasColumnName("login").HasMaxLength(256).IsRequired();
		builder.HasIndex(x => x.Login).IsUnique();
		builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
		builder.Property(x => x.Role).HasColumnName("role").IsRequired().HasConversion<string>();
	}
}