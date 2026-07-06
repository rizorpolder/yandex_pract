using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using yandex_pract.CustomEventService.Models;

namespace yandex_pract.DbContext.DataAccess.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
	public void Configure(EntityTypeBuilder<Event> builder)
	{
		builder.ToTable("events");
		builder.HasKey(e => e.Id);
		builder.Property(p => p.Id).ValueGeneratedNever();
		builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
		builder.Property(e => e.Description).HasMaxLength(255).IsRequired(false);
		builder.HasMany(e => e.Bookings)
			.WithOne(b => b.Event)
			.HasForeignKey(b => b.EventId);
	}
}