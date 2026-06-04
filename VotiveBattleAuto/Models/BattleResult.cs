namespace VotiveBattleAuto.Models;

public sealed class ContestRoll
{
    public int AttackerRoll { get; set; }
    public int DefenderRoll { get; set; }
    public int AttackerBonus { get; set; }
    public int DefenderBonus { get; set; }
    public int AttackerTotal => AttackerRoll + AttackerBonus;
    public int DefenderTotal => DefenderRoll + DefenderBonus;
}

public sealed class ActionResult
{
    public string Text { get; set; } = "";
    public List<ContestRoll> Rolls { get; set; } = new();
    public bool Success { get; set; }
    public int Damage { get; set; }
    public List<string> AffectedTargetIds { get; set; } = new();
}

public sealed class BattleAction
{
    public string ActorId { get; set; } = "";
    public BattleActionType Type { get; set; }
    public string? AbilityId { get; set; }
    public List<string> TargetIds { get; set; } = new();
}
