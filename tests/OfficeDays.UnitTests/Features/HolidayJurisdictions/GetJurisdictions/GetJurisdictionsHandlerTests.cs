using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OfficeDays.Data;
using OfficeDays.Domain;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions;
using OfficeDays.Features.HolidayJurisdictions.GetJurisdictions.Contracts;

namespace OfficeDays.UnitTests.Features.HolidayJurisdictions.GetJurisdictions;

public sealed class GetJurisdictionsHandlerTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new SqliteConnection("Data Source=:memory:");
    private readonly AppDbContext _dbContext;

    public GetJurisdictionsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _dbContext = new AppDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _connection.OpenAsync(cancellationToken);
        await _dbContext.Database.MigrateAsync(cancellationToken);
        await _dbContext.HolidayJurisdictions.ExecuteDeleteAsync(cancellationToken);
    }

    [Fact]
    public async Task Should_ReturnJurisdictionsOrderedByName_WhenJurisdictionsExist()
    {
        // Arrange
        const int expectedStatusCode = StatusCodes.Status200OK;
        GetJurisdictionsItem[] expectedJurisdictions =
        [
            new GetJurisdictionsItem("AT", "Austria"),
            new GetJurisdictionsItem("DE", "Germany"),
            new GetJurisdictionsItem("CH", "Switzerland")
        ];
        var cancellationToken = TestContext.Current.CancellationToken;
        _dbContext.HolidayJurisdictions.AddRange(
            new HolidayJurisdiction { Code = "CH", Name = "Switzerland" },
            new HolidayJurisdiction { Code = "DE", Name = "Germany" },
            new HolidayJurisdiction { Code = "AT", Name = "Austria" });
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.ChangeTracker.Clear();
        var sut = new GetJurisdictionsHandler(_dbContext);

        // Act
        var result = await sut.Handle(new GetJurisdictionsRequest(), cancellationToken);

        // Assert
        var response = Assert.IsType<Ok<GetJurisdictionsResponse>>(result);
        var body = Assert.IsType<GetJurisdictionsResponse>(response.Value);
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Equal(expectedJurisdictions, body);
        Assert.Empty(_dbContext.ChangeTracker.Entries<HolidayJurisdiction>());
    }

    [Fact]
    public async Task Should_ReturnEmptyCollection_WhenNoJurisdictionsExist()
    {
        // Arrange
        const int expectedStatusCode = StatusCodes.Status200OK;
        var sut = new GetJurisdictionsHandler(_dbContext);

        // Act
        var result = await sut.Handle(new GetJurisdictionsRequest(), TestContext.Current.CancellationToken);

        // Assert
        var response = Assert.IsType<Ok<GetJurisdictionsResponse>>(result);
        var body = Assert.IsType<GetJurisdictionsResponse>(response.Value);
        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.Empty(body);
    }

    [Fact]
    public async Task Should_PropagateCancellation_WhenRequestIsCancelled()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: true);
        var sut = new GetJurisdictionsHandler(_dbContext);

        // Act
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.Handle(new GetJurisdictionsRequest(), cancellationToken).AsTask());

        // Assert
        Assert.Equal(cancellationToken, exception.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
