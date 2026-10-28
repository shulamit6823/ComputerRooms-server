namespace App.Core.DTOs.Manager;

public class ResponsibleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool RequiresPasswordChange { get; set; }
    public DateTime CreatedAt { get; set; }
}
