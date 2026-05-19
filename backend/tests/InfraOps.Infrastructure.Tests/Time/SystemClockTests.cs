using InfraOps.Infrastructure.Time;

namespace InfraOps.Infrastructure.Tests.Time;

public sealed class SystemClockTests
{
    [Fact]
    public void Should_ReturnUtcTimeCloseToCurrentSystemTime()
    {
        var clock = new SystemClock();
        var lowerBound = DateTimeOffset.UtcNow.AddSeconds(-1);

        var result = clock.UtcNow;

        Assert.InRange(result, lowerBound, DateTimeOffset.UtcNow.AddSeconds(1));
    }
}
