using NSubstitute;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Attendance.RecordDate;
using OfficeDays.Features.Attendance.RecordDate.Contracts;
using OfficeDays.Infrastructure;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Attendance.RecordDate;

public sealed class RecordDateRequestValidatorTests
{
    [Fact] 
    public async Task Should_ReturnSuccess_WhenNoEntryForThatDate()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        var sut = new RecordDateRequestValidator(TimeProvider.System,
                                                 Substitute.For<ICurrentUser>(),
                                                 Substitute.For<IAppDbContext>(),
                                                 Substitute.For<IUserDateService>());
        var request = new RecordDateRequest
        {
            Date = expectedDate
        };
    
        // act
        var result = await sut.ValidateAsync(request);
    
        // assert
        Assert.True(result.IsValid);
    }
    
    [Fact] 
    public async Task Should_ReturnFailure_WhenNoEntryUser_ButDateInTheFuture()
    {
        // arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
        var sut = new RecordDateRequestValidator(TimeProvider.System,
                                                 Substitute.For<ICurrentUser>(),
                                                 Substitute.For<IAppDbContext>(),
                                                 Substitute.For<IUserDateService>());
        var request = new RecordDateRequest
        {
            Date = expectedDate
        };
    
        // act
        var result = await sut.ValidateAsync(request);
    
        // assert
        Assert.False(result.IsValid);
    }
    
    [Fact] 
    public async Task Should_ReturnSuccess_WhenUserExistsAndLessThanToday()
    {
        // arrange
        var expectedUid = Guid.NewGuid();
        var cu = Substitute.For<ICurrentUser>();
        var db = Substitute.For<IAppDbContext>();
        var us = Substitute.For<IUserDateService>();
        var user = new User
        {
            Username = "test",
            NormalizedUsername = "test",
            PasswordHash = "test-pw",
            TimeZoneId = "ro"
        };
        cu.Id.Returns(expectedUid);
        db.FindAsync<User>([expectedUid], Arg.Any<CancellationToken>())
          .Returns(ValueTask.FromResult<User?>(user));
        us.Today(user).Returns(DateOnly.FromDateTime(DateTime.Now));
        var expectedDate = DateOnly.FromDateTime(DateTime.Now);
        var sut = new RecordDateRequestValidator(TimeProvider.System,
                                                 cu, db, us);
        var request = new RecordDateRequest
        {
            Date = expectedDate
        };
    
        // act
        var result = await sut.ValidateAsync(request);
    
        // assert
        Assert.True(result.IsValid);
    }
    
    [Fact] 
    public async Task Should_ReturnFailure_WhenUserExistsAndGreaterThanToday()
    {
        // arrange
        var expectedUid = Guid.NewGuid();
        var cu = Substitute.For<ICurrentUser>();
        var db = Substitute.For<IAppDbContext>();
        var us = Substitute.For<IUserDateService>();
        var user = new User
        {
            Username = "test",
            NormalizedUsername = "test",
            PasswordHash = "test-pw",
            TimeZoneId = "ro"
        };
        cu.Id.Returns(expectedUid);
        db.FindAsync<User>([expectedUid], Arg.Any<CancellationToken>())
          .Returns(ValueTask.FromResult<User?>(user));
        us.Today(user).Returns(DateOnly.FromDateTime(DateTime.Now));
        var expectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
        var sut = new RecordDateRequestValidator(TimeProvider.System,
                                                 cu, db, us);
        var request = new RecordDateRequest
        {
            Date = expectedDate
        };
    
        // act
        var result = await sut.ValidateAsync(request);
    
        // assert
        Assert.False(result.IsValid);
    }
}
