using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrikeDefender.Application.Common.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalAttacksAsync(CancellationToken cancellationToken);
        Task<int> GetSuccessfulAttacksAsync(CancellationToken cancellationToken);

        Task<int> GetTotalRulesAsync(CancellationToken cancellationToken);
        Task<int> GetActiveRulesAsync(CancellationToken cancellationToken);

        Task<int> GetBlockedAttacksAsync(CancellationToken cancellationToken);
        Task<int> GetExecutedAttacksAsync(CancellationToken cancellationToken);

        Task<double> GetAverageExecutionTimeAsync(CancellationToken cancellationToken);
        Task<string> GetTopAttackTypeAsync(CancellationToken cancellationToken);
    }
}
