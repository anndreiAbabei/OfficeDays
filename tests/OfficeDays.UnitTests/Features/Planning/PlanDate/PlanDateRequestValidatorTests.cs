using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.Planning.PlanDate.Contracts;
using OfficeDays.Features.Planning.PlanDate;
using OfficeDays.Security;
using OfficeDays.Services;

namespace OfficeDays.UnitTests.Features.Planning.PlanDate;

public sealed class PlanDateRequestValidatorTests
{
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public async Task Should_ValidateDateAgainstUsersToday(int offset, bool expectedIsValid)
    {
        // arrange
        var expectedToday = new DateOnly(2026, 10, 19);
        var user = new User { Id = Guid.NewGuid(), Username = "alice", NormalizedUsername = "ALICE", PasswordHash = "hash", TimeZoneId = "Pacific/Auckland" };
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=:memory:").Options);
        db.Users.Attach(user);
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(user.Id);
        var dates = Substitute.For<IUserDateService>();
        dates.Today(user).Returns(expectedToday);
        var request = new PlanDateRequest { Date = expectedToday.AddDays(offset) };
        var sut = new PlanDateRequestValidator(db, currentUser, dates);

        // act
        var result = await sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(expectedIsValid, result.IsValid);
        dates.Received(1).Today(user);
        if (!expectedIsValid)
        {
            var error = Assert.Single(result.Errors);
            Assert.Equal("Date", error.PropertyName);
            Assert.Equal("Past plans cannot be changed.", error.ErrorMessage);
        }
    }

    [Fact]
    public async Task Should_ReturnValidationSuccess_WhenUserDoesNotExist()
    {
        // arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(Guid.NewGuid());
        var dates = Substitute.For<IUserDateService>();
        var request = new PlanDateRequest { Date = new DateOnly(2020, 1, 1) };
        var sut = new PlanDateRequestValidator(db, currentUser, dates);

        // act
        var result = await sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // assert
        Assert.True(result.IsValid);
        dates.DidNotReceive().Today(Arg.Any<User>());
    }
}
