namespace App.Services.Settings;

/// <summary>פרטי המנהל הראשוני שנוצר ב־Seed</summary>
public class InitialManagerSettings
{
    public const string SectionName = "InitialManager";

    public string Name { get; set; } = null!;

    /// <summary>המייל שאיתו המנהל מתחבר</summary>
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}
