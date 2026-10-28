using App.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestMessage> RequestMessages => Set<RequestMessage>();
    public DbSet<TreatmentHistory> TreatmentHistories => Set<TreatmentHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // כל הגדרות ה־Fluent API נמצאות בתיקיית Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
