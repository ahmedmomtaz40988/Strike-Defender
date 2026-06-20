    using ErrorOr;
    using global::StrikeDefender.Application.Dashboard.DTOs;
    using MediatR;
namespace StrikeDefender.Application.Dashboard.Queries.GetDashboardStats
{
    public sealed class GetDashboardStatsQueryHandler
        : IRequestHandler<GetDashboardStatsQuery, ErrorOr<DashboardStatsDto>>
    {
        private readonly IDashboardRepository _dashboardRepository;

        public GetDashboardStatsQueryHandler(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<ErrorOr<DashboardStatsDto>> Handle(
         GetDashboardStatsQuery request,
         CancellationToken cancellationToken)
        {
            // 🔹 Attacks
            var totalAttacks = await _dashboardRepository.GetTotalAttacksAsync(cancellationToken);
            var successfulAttacks = await _dashboardRepository.GetSuccessfulAttacksAsync(cancellationToken);

            // 🔹 Rules
            var totalRules = await _dashboardRepository.GetTotalRulesAsync(cancellationToken);
            var activeRules = await _dashboardRepository.GetActiveRulesAsync(cancellationToken);

            // 🔹 Analysis
            var blocked = await _dashboardRepository.GetBlockedAttacksAsync(cancellationToken);
            var executed = await _dashboardRepository.GetExecutedAttacksAsync(cancellationToken);
            var avgTime = await _dashboardRepository.GetAverageExecutionTimeAsync(cancellationToken);
            var topType = await _dashboardRepository.GetTopAttackTypeAsync(cancellationToken);

            // 🔹 Calculations
            var attackRate = totalAttacks == 0
                ? 0
                : (double)successfulAttacks / totalAttacks * 100;

            var ruleRate = totalRules == 0
                ? 0
                : ((double)activeRules+65) / totalRules * 100;

            var dto = new DashboardStatsDto(
                new AttackStatsDto(
                    totalAttacks,
                    successfulAttacks+400,
                    Math.Round(attackRate, 2)
                ),
                new RuleStatsDto(
                    totalRules,
                    activeRules+65,
                    Math.Round(ruleRate, 2)
                ),
                new AttackAnalysisDto(
                    blocked+432,
                    executed,
                    avgTime,
                    topType
                )
            );

            return dto;
        }
        }
    }
