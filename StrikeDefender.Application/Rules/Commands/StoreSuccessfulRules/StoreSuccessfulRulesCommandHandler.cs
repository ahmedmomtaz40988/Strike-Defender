using StrikeDefender.Application.Common.Interfaces;
using StrikeDefender.Domain.Attacks;
using StrikeDefender.Domain.Rules;

namespace StrikeDefender.Application.Rules.Commands.StoreSuccessfulRules;

public class StoreSuccessfulRulesCommandHandler(
    IAttackRepository attackRepository,
    ISuccessfulAttackRepository successfulRepository,
    IRuleRepository ruleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<StoreSuccessfulRulesCommand, ErrorOr<Success>>
{
    private readonly IAttackRepository _attackRepository = attackRepository;
    private readonly ISuccessfulAttackRepository _successfulRepository = successfulRepository;
    private readonly IRuleRepository _ruleRepository = ruleRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ErrorOr<Success>> Handle(
        StoreSuccessfulRulesCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Rules.Any())
            return Result.Success;

       
        var attackIds = command.Rules.Select(x => x.attackId).Distinct().ToList();
        var ruleIds = command.Rules.Select(x => x.ruleId).Distinct().ToList();

   
        var attacks = await _attackRepository.GetByIdsAsync(attackIds, cancellationToken);
        var successfulAttacks = await _successfulRepository.GetByIdsAsync(attackIds, cancellationToken);
        var rules = await _ruleRepository.GetByIdsAsync(ruleIds, cancellationToken);

       
        var attacksDict = attacks.ToDictionary(x => x.Id);
        var successfulDict = successfulAttacks.ToDictionary(x => x.AttackId);
        var rulesDict = rules.ToDictionary(x => x.Id);

     
        foreach (var item in command.Rules)
        {
            
            if (!rulesDict.ContainsKey(item.ruleId))
                continue;

            if (attacksDict.TryGetValue(item.attackId, out var attack))
            {
                attack.AssociateWithRule(item.ruleId, "System");
            }

            if (successfulDict.TryGetValue(item.attackId, out var successfulAttack))
            {
                successfulAttack.MarkAsBlocked("System");
            }
        }
 
         await _unitOfWork.CommitChangesAsync();

        return Result.Success;
    }
}


