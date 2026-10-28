using App.Core.Enums;

namespace App.Core.DTOs.Requests;

public class MessageDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = null!;
    public UserRole AuthorRole { get; set; }
    public string Description { get; set; } = null!;
    public MessageVisibility Visibility { get; set; }
}
