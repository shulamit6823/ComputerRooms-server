using App.Api.Authorization;
using App.Api.Extensions;
using App.Core.DTOs.Requests;
using App.Core.Enums;
using App.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize(Roles = Roles.TeacherOrResponsible)]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    // ---------- מורה ----------

    [HttpPost]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<TeacherRequestDetailsDto>> Create(CreateRequestDto dto)
    {
        var created = await _requestService.CreateAsync(User.GetUserId(), dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("mine")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<List<TeacherRequestListItemDto>>> GetMine() =>
        Ok(await _requestService.GetMyRequestsAsync(User.GetUserId()));

    /// <summary>
    /// עדכון סוג ותאריך יעד - רק המורה שפתחה את הפנייה, וכשהיא ממתינה לטיפול.
    /// זו הדרך היחידה לשנות תאריך יעד.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<TeacherRequestDetailsDto>> Update(int id, UpdateRequestDto dto) =>
        Ok(await _requestService.UpdateByTeacherAsync(id, User.GetUserId(), dto));

    /// <summary>"הסתיימה" → "ממתינה לטיפול"</summary>
    [HttpPost("{id:int}/reopen")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<TeacherRequestDetailsDto>> Reopen(int id) =>
        Ok(await _requestService.ReopenAsync(id, User.GetUserId()));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<IActionResult> Delete(int id)
    {
        await _requestService.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }

    // ---------- אחראית ----------

    /// <summary>כל הפניות, עם חיפוש, סינון ומיון</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<List<RequestListItemDto>>> GetAll([FromQuery] RequestQueryDto query) =>
        Ok(await _requestService.GetAllAsync(query));

    /// <summary>הפניות שהאחראית המחוברת מטפלת בהן</summary>
    [HttpGet("assigned")]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<List<RequestListItemDto>>> GetAssigned() =>
        Ok(await _requestService.GetAssignedAsync(User.GetUserId()));

    /// <summary>שינוי סוג פנייה בלבד</summary>
    [HttpPut("{id:int}/type")]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<RequestDetailsDto>> ChangeType(int id, ChangeRequestTypeDto dto) =>
        Ok(await _requestService.ChangeTypeAsync(id, User.GetUserId(), dto));

    [HttpPost("{id:int}/take")]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<RequestDetailsDto>> Take(int id) =>
        Ok(await _requestService.TakeAsync(id, User.GetUserId()));

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<RequestDetailsDto>> Complete(int id) =>
        Ok(await _requestService.CompleteAsync(id, User.GetUserId()));

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<RequestDetailsDto>> ChangeStatus(int id, ChangeStatusDto dto) =>
        Ok(await _requestService.ChangeStatusAsync(id, User.GetUserId(), dto.Status!.Value));

    [HttpPost("{id:int}/internal-notes")]
    [Authorize(Roles = Roles.Responsible)]
    public async Task<ActionResult<MessageDto>> AddInternalNote(int id, AddInternalNoteDto dto) =>
        Ok(await _requestService.AddInternalNoteAsync(id, User.GetUserId(), dto));

    // ---------- מורה ואחראית ----------

    /// <summary>רשימת סוגי הפניות בעברית, לפי סדר ה־enum - אלה הערכים שהפרונט שולח בשדה requestType</summary>
    [HttpGet("types")]
    public ActionResult<List<string>> GetTypes() =>
        Ok(Enum.GetValues<RequestType>().Select(type => DisplayNames.RequestTypes[type]).ToList());

    /// <summary>מורה מקבלת תצוגה מצומצמת, אחראית מקבלת את כל הפרטים</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (User.GetRole() == UserRole.Teacher)
            return Ok(await _requestService.GetForTeacherAsync(id, User.GetUserId()));

        return Ok(await _requestService.GetForResponsibleAsync(id));
    }

    /// <summary>שורה חדשה במערך התיאור, גלויה למורה</summary>
    [HttpPost("{id:int}/messages")]
    public async Task<ActionResult<MessageDto>> AddMessage(int id, AddMessageDto dto) =>
        Ok(await _requestService.AddMessageAsync(id, User.GetUserId(), User.GetRole(), dto));
}
