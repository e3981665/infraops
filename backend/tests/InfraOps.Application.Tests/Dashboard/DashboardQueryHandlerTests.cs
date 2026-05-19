using InfraOps.Application.Abstractions.Time;
using InfraOps.Application.Dashboard.Abstractions;
using InfraOps.Application.Dashboard.Dtos;
using InfraOps.Application.Dashboard.Queries.GetDashboardSummary;
using InfraOps.Application.Dashboard.Queries.GetExecutionMetrics;
using InfraOps.Application.Dashboard.Support;
using FluentValidation;

namespace InfraOps.Application.Tests.Dashboard;

public sealed class DashboardQueryHandlerTests
{
    [Fact]
    public async Task Should_ReturnDashboardSummary_When_FilterIsValid()
    {
        var repository = new StubDashboardMetricsRepository();
        var handler = new GetDashboardSummaryQueryHandler(
            new GetDashboardSummaryQueryValidator(),
            repository,
            new StubClock());

        var result = await handler.Handle(
            new GetDashboardSummaryQuery(
                StubDashboardMetricsRepository.RegionId,
                null,
                null,
                null,
                null),
            CancellationToken.None);

        Assert.Equal(6, result.TotalInventoryItems);
        Assert.Equal(StubDashboardMetricsRepository.RegionId, repository.LastFilter!.RegionId);
        Assert.Equal(StubClock.Now, repository.LastNowUtc);
    }

    [Fact]
    public async Task Should_RejectDashboardQuery_When_DateRangeIsInvalid()
    {
        var handler = new GetExecutionMetricsQueryHandler(
            new GetExecutionMetricsQueryValidator(),
            new StubDashboardMetricsRepository());

        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(
            new GetExecutionMetricsQuery(null, null, null, StubClock.Now, StubClock.Now.AddDays(-1)),
            CancellationToken.None));
    }

    private sealed class StubDashboardMetricsRepository : IDashboardMetricsRepository
    {
        public static readonly Guid RegionId = Guid.Parse("8F868090-ADDF-4366-9946-5B418574C115");

        public DashboardFilter? LastFilter { get; private set; }

        public DateTimeOffset? LastNowUtc { get; private set; }

        public Task<DashboardSummaryDto> GetSummaryAsync(
            DashboardFilter filter,
            DateTimeOffset nowUtc,
            CancellationToken cancellationToken)
        {
            LastFilter = filter;
            LastNowUtc = nowUtc;

            return Task.FromResult(new DashboardSummaryDto(6, 6, 3, 1, 2, 1, 1, 3, 3));
        }

        public Task<ExecutionMetricsDto> GetExecutionMetricsAsync(
            DashboardFilter filter,
            CancellationToken cancellationToken)
        {
            LastFilter = filter;

            return Task.FromResult(new ExecutionMetricsDto(6, 1, 1, 2, 1, 1, []));
        }

        public Task<ValidationMetricsDto> GetValidationMetricsAsync(
            DashboardFilter filter,
            CancellationToken cancellationToken)
        {
            LastFilter = filter;

            return Task.FromResult(new ValidationMetricsDto(1, 2, 1, 1, []));
        }

        public Task<InventoryMetricsDto> GetInventoryMetricsAsync(
            DashboardFilter filter,
            CancellationToken cancellationToken)
        {
            LastFilter = filter;

            return Task.FromResult(new InventoryMetricsDto(6, 6, []));
        }

        public Task<DashboardChartsDto> GetChartsAsync(
            DashboardFilter filter,
            CancellationToken cancellationToken)
        {
            LastFilter = filter;

            return Task.FromResult(new DashboardChartsDto([], [], [], []));
        }
    }

    private sealed class StubClock : IClock
    {
        public static readonly DateTimeOffset Now = new(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);

        public DateTimeOffset UtcNow => Now;
    }
}
