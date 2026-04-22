using CaseManagement.Application.Cases.Services;
using CaseManagement.Domain.Entities;
using FluentAssertions;

namespace CaseManagement.Api.Tests;

public sealed class DueSoonWindowMatcherTests
{
    [Fact]
    public void MatchWindow_prefers_4h_band()
    {
        var now = DateTimeOffset.UtcNow;
        var due = now.AddHours(2);

        var window = DueSoonWindowMatcher.MatchWindow(now, due, [240, 60, 15]);

        window.Should().Be(240);
    }

    [Fact]
    public void MatchWindow_prefers_1h_band()
    {
        var now = DateTimeOffset.UtcNow;
        var due = now.AddMinutes(45);

        var window = DueSoonWindowMatcher.MatchWindow(now, due, [240, 60, 15]);

        window.Should().Be(60);
    }

    [Fact]
    public void MatchWindow_prefers_15m_band()
    {
        var now = DateTimeOffset.UtcNow;
        var due = now.AddMinutes(10);

        var window = DueSoonWindowMatcher.MatchWindow(now, due, [240, 60, 15]);

        window.Should().Be(15);
    }

    [Fact]
    public void IsSuppressed_true_for_paused_and_breached_and_terminal_and_unassigned()
    {
        var policy = new SlaDurationPolicy(24, 8, 4);
        var orgId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();

        var paused = Case.Create(orgId, "Paused", actorId, "Body", policy, CasePriority.Medium);
        paused.AssignTo(assigneeId, actorId);
        paused.ChangeStatus(CaseStatus.Pending, actorId);

        var breached = Case.Create(orgId, "Breached", actorId, "Body", new SlaDurationPolicy(0, 0, 0), CasePriority.Medium);
        breached.AssignTo(assigneeId, actorId);
        breached.ChangeStatus(CaseStatus.Open, actorId);

        var resolved = Case.Create(orgId, "Resolved", actorId, "Body", policy, CasePriority.Medium);
        resolved.AssignTo(assigneeId, actorId);
        resolved.ChangeStatus(CaseStatus.Resolved, actorId);

        var unassigned = Case.Create(orgId, "Unassigned", actorId, "Body", policy, CasePriority.Medium);

        DueSoonWindowMatcher.IsSuppressed(paused).Should().BeTrue();
        DueSoonWindowMatcher.IsSuppressed(breached).Should().BeTrue();
        DueSoonWindowMatcher.IsSuppressed(resolved).Should().BeTrue();
        DueSoonWindowMatcher.IsSuppressed(unassigned).Should().BeTrue();
    }
}
