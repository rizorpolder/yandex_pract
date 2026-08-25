using EventsService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventsService.Infrastructure.Mapping;

public class ProcessMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
	public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
	{
		builder.ToTable("processed_messages");
		builder.HasKey(x => x.BookingId);
		builder.Property(x => x.BookingId)
			.HasColumnName("booking_id")
			.ValueGeneratedNever();
		builder.Property(x => x.ProcessedAt).HasColumnName("processed_at");

	}
}