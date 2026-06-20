using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StrikeDefender.Application.Dashboard.Queries.DeleteUser;
using StrikeDefender.Application.Dashboard.Queries.GetDashboardStats;

namespace StrikeDefender.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(ISender sender) : ApiController
    {
        private readonly ISender _Mediator = sender;


        [HttpGet("SecurityOverview")]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var result = await _Mediator.Send(
                new GetDashboardStatsQuery(),
                cancellationToken);

            return result.Match(
                    data => Ok(data),
                    errors => ToProblem(errors));
        }

        [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUserByEmail(string email, CancellationToken cancellationToken)
        {
            var result = await _Mediator.Send(new DeleteUserbyEmailQuery(email), cancellationToken);

            return result.Match(
                deleted => Ok(),
                errors => ToProblem(errors));
        }
    }
}
