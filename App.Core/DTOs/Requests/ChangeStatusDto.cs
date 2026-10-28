using System.ComponentModel.DataAnnotations;
using App.Core.Enums;

namespace App.Core.DTOs.Requests;

public class ChangeStatusDto
{
    [Required(ErrorMessage = "סטטוס הוא שדה חובה")]
    public RequestStatus? Status { get; set; }
}
