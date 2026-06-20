
using StrikeDefender.Application.Dashboard.DTOs;

namespace StrikeDefender.Application.Dashboard.Queries.GetDashboardStats
{
    public sealed record GetDashboardStatsQuery()
        : IRequest<ErrorOr<DashboardStatsDto>>;
}
