using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.ComputerNumber).HasMaxLength(50);
        builder.Property(r => r.RequestType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();
        builder.Property(r => r.DueDate).IsRequired().HasColumnType("date");

        // Concurrency: עמודת המערכת xmin של PostgreSQL משתנה בכל עדכון של השורה.
        // אם שתי אחראיות מעדכנות את אותה פנייה במקביל, השנייה תיכשל.
        builder.UseXminAsConcurrencyToken();

        builder.HasOne(r => r.Teacher)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CurrentResponsible)
            .WithMany()
            .HasForeignKey(r => r.CurrentResponsibleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Messages)
            .WithOne(m => m.Request)
            .HasForeignKey(m => m.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.TreatmentHistory)
            .WithOne(t => t.Request)
            .HasForeignKey(t => t.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.TeacherId);
        builder.HasIndex(r => r.CurrentResponsibleId);
        builder.HasIndex(r => r.Status);
    }
}
