using Microsoft.EntityFrameworkCore;
using StrikeDefender.Application.Common.Interfaces;
using StrikeDefender.Domain.Attacks;
using StrikeDefender.Domain.Common.Enums;
using StrikeDefender.Domain.Rules;
using StrikeDefender.Infrastructure.Common.Persistence.Data;

namespace StrikeDefender.Infrastructure.Dashboard
{
    public sealed class DashboardRepository : IDashboardRepository
    {
        private readonly StrikeDefenderDbContext _context;

        public DashboardRepository(StrikeDefenderDbContext context)
        {
            _context = context;
        }

        public Task<int> GetTotalAttacksAsync(CancellationToken cancellationToken) =>
            _context.Set<Attack>()
                .AsNoTracking()
                .CountAsync(a => !a.Deleted, cancellationToken);

        public Task<int> GetSuccessfulAttacksAsync(CancellationToken cancellationToken) =>
            _context.Set<AttackResult>()
                .AsNoTracking()
                .CountAsync(r => r.IsSuccessful && !r.Deleted, cancellationToken);

        public Task<int> GetTotalRulesAsync(CancellationToken cancellationToken) =>
            _context.Set<WafRule>()
                .AsNoTracking()
                .CountAsync(r => !r.Deleted, cancellationToken);

        public Task<int> GetActiveRulesAsync(CancellationToken cancellationToken) =>
            _context.Set<WafRule>()
                .AsNoTracking()
                .CountAsync(r =>
                    r.Status == RuleStatus.Active &&
                    !r.Deleted,
                    cancellationToken);

        public Task<int> GetBlockedAttacksAsync(CancellationToken cancellationToken) =>
            _context.Set<AttackResult>()
                .AsNoTracking()
                .CountAsync(r =>
                    r.IsBlockedByWaf &&
                    !r.Deleted,
                    cancellationToken);

        public Task<int> GetExecutedAttacksAsync(CancellationToken cancellationToken) =>
            _context.Set<AttackResult>()
                .AsNoTracking()
                .CountAsync(r =>
                    r.IsExecuted &&
                    !r.Deleted,
                    cancellationToken);
        public async Task<double> GetAverageExecutionTimeAsync(CancellationToken cancellationToken)
        {
            var query = _context.Set<AttackResult>()
                .AsNoTracking()
                .Where(r => !r.Deleted);

            if (!await query.AnyAsync(cancellationToken))
                return 0;

            var avg = await query
                .AverageAsync(r => r.ExecutionTimeMs, cancellationToken);

            return Math.Round(avg, 2);
        }

        public async Task<string> GetTopAttackTypeAsync(CancellationToken cancellationToken)
        {
            var result = await _context.Set<Attack>()
                .AsNoTracking()
                .Where(a => !a.Deleted)
                .GroupBy(a => a.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(cancellationToken);

            return result?.Type.ToString() ?? "N/A";
        }
    }
}
