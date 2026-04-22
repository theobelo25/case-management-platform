using CaseManagement.Domain.Entities;
using FluentAssertions;

namespace CaseManagement.Domain.Tests;

public sealed class CaseAggregateTests
{
    [Fact]
    public void Create_sets_sla_due_from_policy_for_priority()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var policy = new SlaDurationPolicy(LowHours: 24, MediumHours: 8, HighHours: 4);

        var @case = Case.Create(
            orgId,
            "Title",
            userId,
            "Initial message body.",
            policy,
            CasePriority.High);

        var expected = TimeSpan.FromHours(4);
        (@case.SlaDueAtUtc!.Value - @case.CreatedAtUtc).Should().Be(expected);
        @case.SlaBreachedAtUtc.Should().BeNull();
        @case.SlaPausedAtUtc.Should().BeNull();
        @case.SlaRemainingSeconds.Should().Be((int)expected.TotalSeconds);
    }

    [Fact]
    public void Pending_pauses_clock_Open_resumes_with_remaining_seconds()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var policy = new SlaDurationPolicy(24, 8, 4);

        var @case = Case.Create(
            orgId,
            "Title",
            userId,
            "Initial message body.",
            policy,
            CasePriority.Medium);

        @case.ChangeStatus(CaseStatus.Pending, userId);

        @case.Status.Should().Be(CaseStatus.Pending);
        @case.SlaPausedAtUtc.Should().NotBeNull();
        @case.SlaRemainingSeconds.Should().NotBeNull();
        @case.SlaRemainingSeconds!.Value.Should().BeGreaterThan(0);

        @case.ChangeStatus(CaseStatus.Open, userId);

        @case.SlaPausedAtUtc.Should().BeNull();
        @case.SlaRemainingSeconds.Should().BeNull();
        @case.SlaDueAtUtc.Should().NotBeNull();
        @case.SlaDueAtUtc!.Value.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void ChangePriority_with_policy_reanchors_due_from_now()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var policy = new SlaDurationPolicy(LowHours: 24, MediumHours: 8, HighHours: 4);

        var @case = Case.Create(
            orgId,
            "Title",
            userId,
            "Initial message body.",
            policy,
            CasePriority.Medium);

        @case.ChangePriority(CasePriority.High, policy, userId);

        @case.Priority.Should().Be(CasePriority.High);
        var expected = TimeSpan.FromHours(4);
        (@case.SlaDueAtUtc!.Value - @case.UpdatedAtUtc).Should().BeCloseTo(expected, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Recalculate_after_due_emits_sla_breached_event()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var zeroPolicy = new SlaDurationPolicy(0, 0, 0);

        var @case = Case.Create(
            orgId,
            "Title",
            userId,
            "Initial message body.",
            zeroPolicy,
            CasePriority.Medium);

        @case.ChangeStatus(CaseStatus.Open, userId);

        @case.SlaBreachedAtUtc.Should().NotBeNull();
        @case.Events.Should().Contain(e => e.Type == "sla_breached");
    }
}
