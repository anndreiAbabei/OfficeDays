using Microsoft.Extensions.Options;
using OfficeDays.Security.RateLimiting;

namespace OfficeDays.UnitTests.Security.RateLimiting;

public sealed class RequestLimitStoreTests
{
    [Fact]
    public void Should_RecoverAtWindowBoundary_WhenLimitIsExhausted()
    {
        // Arrange
        var clock = new MutableClock();
        var store = new RequestLimitStore(clock, Options.Create(new RequestLimits()));
        var limit = new RequestLimit(1, 60);
        const int expectedAllowed = 0;
        const int expectedRetry = 60;

        // Act / Assert
        Assert.Equal(expectedAllowed, store.Acquire("a", limit));
        Assert.Equal(expectedRetry, store.Acquire("a", limit));
        Assert.Equal(expectedAllowed, store.Acquire("b", limit));
        clock.Now = clock.Now.AddSeconds(30);
        const int expectedRemainingRetry = 30;
        Assert.Equal(expectedRemainingRetry, store.Acquire("a", limit));
        clock.Now = clock.Now.AddSeconds(30);
        Assert.Equal(expectedAllowed, store.Acquire("a", limit));
    }

    [Fact]
    public async Task Should_AdmitExactlyTheAllowance_WhenRequestsAreConcurrent()
    {
        // Arrange
        var store = new RequestLimitStore(new MutableClock(), Options.Create(new RequestLimits()));
        const int expectedAllowed = 10;
        var limit = new RequestLimit(expectedAllowed, 60);

        // Act
        var results = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => Task.Run(() => store.Acquire("a", limit))));

        // Assert
        Assert.Equal(expectedAllowed, results.Count(x => x == 0));
    }

    [Fact]
    public void Should_FailClosedAndReleaseExpiredPartitions_WhenCapacityIsReached()
    {
        // Arrange
        var clock = new MutableClock();
        var store = new RequestLimitStore(clock, Options.Create(new RequestLimits { MaxPartitions = 1 }));
        var limit = new RequestLimit(2, 60);
        const int expectedAllowed = 0;
        const int expectedRetry = 60;

        // Act / Assert
        Assert.Equal(expectedAllowed, store.Acquire("a", limit));
        Assert.Equal(expectedRetry, store.Acquire("b", limit));
        clock.Now = clock.Now.AddSeconds(60);
        Assert.Equal(expectedAllowed, store.Acquire("b", limit));
    }

    private sealed class MutableClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 20, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
