using StrikeDefender.Domain.Attacks;

namespace StrikeDefender.Application.Attacks.Commands.StoreSuccessfulAttacks;

public class StoreSuccessfulAttacksCommandHandler(
        IAttackRepository attackRepository,
        IGenericRepository<AttackResult> resultRepository,
        IGenericRepository<SuccessfulAttack> successfulRepository,
        IUnitOfWork unitOfWork)
    : IRequestHandler<StoreSuccessfulAttacksCommand, ErrorOr<Success>>
{
    private readonly IAttackRepository _attackRepository = attackRepository;
    private readonly IGenericRepository<AttackResult> _resultRepository = resultRepository;
    private readonly IGenericRepository<SuccessfulAttack> _successfulRepository = successfulRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<ErrorOr<Success>> Handle(
        StoreSuccessfulAttacksCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Attacks.Count == 0)
            return Result.Success;

        var attackIds = command.Attacks
            .Select(a => a.AttackId)
            .Distinct()
            .ToList();

        var attacks = await _attackRepository
            .GetByIdsAsync(attackIds, cancellationToken);

        var attackDictionary = attacks.ToDictionary(a => a.Id);

        var resultsToAdd = new List<AttackResult>();
        var successfulToAdd = new List<SuccessfulAttack>();

        foreach (var dto in command.Attacks)
        {
            if (!attackDictionary.TryGetValue(dto.AttackId, out var attack))
                continue;

            var resultOrError = AttackResult.Create(
                attack,
                dto.IsBlockedByWaf,
                true,
                true,
                dto.StatusCode,
                dto.Result,
                dto.ExecutionTimeMs
            );

            if (resultOrError.IsError)
                continue;

            var result = resultOrError.Value;

            var successfulOrError = SuccessfulAttack.Create(
                attack,
                result,
                dto.Technique,
                dto.Target,
                dto.Severity,
                "sandbox"

            );

            if (successfulOrError.IsError)
                continue;

            resultsToAdd.Add(result);
            successfulToAdd.Add(successfulOrError.Value);

            attack.MarkAsSuccessful("sandbox");
        }

        if (resultsToAdd.Count > 0)
        {
            await _resultRepository.AddRangeAsync(resultsToAdd);
            await _successfulRepository.AddRangeAsync(successfulToAdd);
            await _unitOfWork.CommitChangesAsync();
        }

        return Result.Success;
    }
}


