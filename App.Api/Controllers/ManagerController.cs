using App.Api.Authorization;
using App.Core.DTOs.Manager;
using App.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/manager/responsibles")]
[Authorize(Roles = Roles.Manager)]
public class ManagerController : ControllerBase
{
    private readonly IManagerService _managerService;

    public ManagerController(IManagerService managerService)
    {
        _managerService = managerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResponsibleDto>>> GetResponsibles() =>
        Ok(await _managerService.GetResponsiblesAsync());

    /// <summary>יצירת אחראית עם הסיסמה הזמנית 12345678</summary>
    [HttpPost]
    public async Task<ActionResult<ResponsibleDto>> CreateResponsible(CreateResponsibleDto dto) =>
        Ok(await _managerService.CreateResponsibleAsync(dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteResponsible(int id)
    {
        await _managerService.DeleteResponsibleAsync(id);
        return NoContent();
    }
}
