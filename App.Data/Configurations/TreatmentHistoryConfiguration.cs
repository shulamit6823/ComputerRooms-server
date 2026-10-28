using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Data.Configurations;

public class TreatmentHistoryConfiguration : IEntityTypeConfiguration<TreatmentHistory>
{
    public void Configure(EntityTypeBuilder<TreatmentHistory> builder)
    {
        builder.ToTable("TreatmentHistories");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ResponsibleName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.StartedAt).IsRequired();
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.EndReason).HasConversion<string>().HasMaxLength(30);

        // אחראית נמחקת במחיקה רכה בלבד - ההיסטוריה שלה לעולם לא נמחקת
        builder.HasOne(t => t.Responsible)
            .WithMany()
            .HasForeignKey(t => t.ResponsibleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.RequestId);
        builder.HasIndex(t => t.ResponsibleId);
    }
}
