using System.Reflection;
using App.Api.Authorization;
using App.Api.Controllers;
using App.Core.DTOs.Requests;
using App.Core.Entities;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces.Repositories;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace App.Tests;

public class RequestServiceChangeTypeTests
{
    private readonly FakeUserRepository _users = new();
    private readonly Mock<IRequestRepository> _requests = new();
    private readonly RequestService _service;
    private readonly User _teacher;
    private readonly User _responsible;

    public RequestServiceChangeTypeTests()
    {
        _service = new RequestService(_requests.Object, _users);
        _teacher = _users.Seed("מורה", "teacher@test.com", "hash", UserRole.Teacher);
        _responsible = _users.Seed("אחראית", "resp@test.com", "hash", UserRole.Responsible);
    }

    private Request SeedRequest(RequestStatus status)
    {
        var request = new Request
        {
            Id = 1,
            TeacherId = _teacher.Id,
            Teacher = _teacher,
            RoomName = "מעבדה 1",
            RequestType = RequestType.Hardware,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7)
        };
        if (status != RequestStatus.Pending)
        {
            request.CurrentResponsibleId = _responsible.Id;
            request.CurrentResponsible = _responsible;
        }

        _requests.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(request);
        return request;
    }

    [Theory]
    [InlineData(RequestStatus.Pending)]
    [InlineData(RequestStatus.InProgress)]
    [InlineData(RequestStatus.Finished)]
    public async Task ChangeType_ByResponsible_SucceedsInEveryStatus(RequestStatus status)
    {
        var request = SeedRequest(status);

        var result = await _service.ChangeTypeAsync(1, _responsible.Id,
            new ChangeRequestTypeDto { RequestType = RequestType.Network });

        Assert.Equal(RequestType.Network, result.RequestType);
        Assert.Equal(RequestType.Network, request.RequestType);
        Assert.Equal(status, request.Status);
        _requests.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ChangeType_InactiveResponsible_IsRejected()
    {
        var request = SeedRequest(RequestStatus.InProgress);
        var inactive = _users.Seed("לא פעילה", "old@test.com", "hash", UserRole.Responsible, isActive: false);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.ChangeTypeAsync(1, inactive.Id,
            new ChangeRequestTypeDto { RequestType = RequestType.Network }));

        Assert.Equal(RequestType.Hardware, request.RequestType);
        _requests.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public void ChangeTypeEndpoint_AllowsResponsibleOnly()
    {
        var method = typeof(RequestsController).GetMethod(nameof(RequestsController.ChangeType))!;
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>()!;

        Assert.Equal(Roles.Responsible, authorize.Roles);
    }

    [Theory]
    [InlineData(RequestStatus.InProgress)]
    [InlineData(RequestStatus.Finished)]
    public async Task UpdateByTeacher_NotPending_StillRejected(RequestStatus status)
    {
        var request = SeedRequest(status);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.UpdateByTeacherAsync(1, _teacher.Id,
            new UpdateRequestDto { RequestType = RequestType.Network, DueDate = DateTime.UtcNow.AddDays(10) }));

        Assert.Equal(RequestType.Hardware, request.RequestType);
    }
}
