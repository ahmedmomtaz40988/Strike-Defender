using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeDefender.Application.Dashboard.DTOs
{
    public sealed record DashboardStatsDto(
        AttackStatsDto Attacks,
        RuleStatsDto Rules,
        AttackAnalysisDto Analysis
    );

    public sealed record AttackStatsDto(
        int TotalAttacks,
        int SuccessfulAttacks,
        double SuccessRate
    );

    public sealed record RuleStatsDto(
        int TotalRules,
        int ActiveRules,
        double ActiveRate
    );

    public sealed record AttackAnalysisDto(
        int BlockedCount,
        int ExecutedCount,
        double AvgExecutionTime,
        string TopAttackType
    );
}
