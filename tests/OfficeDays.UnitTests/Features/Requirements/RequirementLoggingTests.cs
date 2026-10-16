using Microsoft.Extensions.Logging;
using OfficeDays.Features.Users.CreateRequirement;
using OfficeDays.Features.Users.DeleteRequirement;
using OfficeDays.Features.Users.GetRequirements;
using OfficeDays.Features.Users.UpdateRequirement;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Requirements;

public sealed class RequirementLoggingTests
{
    [Fact]
    public void Missing_user_and_requirement_logs_identify_the_resource_and_user()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var create = new FakeLogger<CreateRequirementHandler>();
        var get = new FakeLogger<GetRequirementsHandler>();
        var update = new FakeLogger<UpdateRequirementHandler>();
        var delete = new FakeLogger<DeleteRequirementHandler>();
        create.LogUserNotFound(userId);
        get.LogUserNotFound(userId);
        update.LogRequirementNotFound(id, userId);
        delete.LogRequirementNotFound(id, userId);
        create.Assert(LogLevel.Warning, $"User {userId} was not found");
        get.Assert(LogLevel.Warning, $"User {userId} was not found");
        update.Assert(LogLevel.Warning, $"Attendance requirement {id} was not found for user {userId}");
        delete.Assert(LogLevel.Warning, $"Attendance requirement {id} was not found for user {userId}");
    }

    [Fact]
    public void Catch_logs_retain_the_exception_and_validation_logs_identify_the_baseline()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var date = new DateOnly(2027, 1, 1);
        var exception = new InvalidOperationException("Duplicate month");
        var create = new FakeLogger<CreateRequirementHandler>();
        var update = new FakeLogger<UpdateRequirementHandler>();
        var delete = new FakeLogger<DeleteRequirementHandler>();
        create.LogDuplicateRequirement(exception, userId, date);
        update.LogDuplicateRequirement(exception, userId, date);
        delete.LogDuplicateRequirement(exception, userId, date);
        foreach (var entry in new[] { create.Logs.Single(), update.Logs.Single(), delete.Logs.Single() })
        {
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Same(exception, entry.Exception);
            Assert.Contains(userId.ToString(), entry.Message);
        }
        update.LogBaselineChangeRejected(id, userId);
        delete.LogBaselineChangeRejected(id, userId);
        update.Assert(LogLevel.Warning, $"Rejected a change to the initial attendance requirement {id} for user {userId}");
        delete.Assert(LogLevel.Warning, $"Rejected a change to the initial attendance requirement {id} for user {userId}");
    }
}
