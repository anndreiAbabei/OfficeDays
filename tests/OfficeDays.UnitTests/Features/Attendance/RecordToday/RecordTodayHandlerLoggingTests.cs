using Microsoft.Extensions.Logging;
using OfficeDays.Features.Attendance.RecordToday;
using OfficeDays.UnitTests.Testing;

namespace OfficeDays.UnitTests.Features.Attendance.RecordToday;

public sealed class RecordTodayHandlerLoggingTests
{
    [Fact]
    public void Should_LogDuplicatedEntryRequest()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedDate = DateOnly.FromDateTime(DateTime.Today);
        var logger = new FakeLogger<RecordTodayHandler>();
        
        // act
        logger.LogDuplicatedEntryRequest(expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Debug, $"Ignored duplicate office attendance for user {expectedUserId} on {expectedDate}");
    }
    
    [Fact]
    public void Should_LogEntryCreated()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedDate = DateOnly.FromDateTime(DateTime.Today);
        var logger = new FakeLogger<RecordTodayHandler>();
        
        // act
        logger.LogEntryCreated(expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Information, $"Recorded office attendance for user {expectedUserId} on {expectedDate}");
    }
    
    [Fact]
    public void Should_LogResolvedConcurrentUpdate()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();
        var expectedDate = DateOnly.FromDateTime(DateTime.Today);
        var logger = new FakeLogger<RecordTodayHandler>();
        
        // act
        logger.LogResolvedConcurrentUpdate(expectedUserId, expectedDate);

        // assert
        logger.Assert(LogLevel.Debug, $"Resolved concurrent duplicate attendance for user {expectedUserId} on {expectedDate}");
    }
}
