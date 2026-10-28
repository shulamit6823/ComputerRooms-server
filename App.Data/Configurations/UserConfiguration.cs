using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        // השם הוא פרט תצוגה בלבד ואינו ייחודי - ההתחברות היא לפי מייל + סיסמה
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);

        // המייל משמש להתחברות ולכן ייחודי בין המשתמשים הפעילים,
        // כדי שאפשר יהיה ליצור אחראית חדשה עם מייל של אחראית שנמחקה (IsActive = false)
        builder.Property(u => u.Email).HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique().HasFilter("\"IsActive\" = true");

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
        builder.Property(u => u.RequiresPasswordChange).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
    }
}
