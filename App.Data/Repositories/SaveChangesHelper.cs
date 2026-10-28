using App.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace App.Data.Repositories;

internal static class SaveChangesHelper
{
    private const string UniqueViolation = "23505";

    /// <summary>מתרגם שגיאות של EF/PostgreSQL ל־Exceptions של המערכת</summary>
    public static async Task SaveAsync(AppDbContext context)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("הפנייה עודכנה במקביל על ידי משתמש אחר. יש לרענן ולנסות שוב.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            throw new ConflictException("הרשומה כבר קיימת במערכת.");
        }
    }
}
