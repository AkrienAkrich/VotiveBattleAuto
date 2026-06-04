namespace VotiveBattleAuto.Models;

public sealed class RaceBonus
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public CombatStats Stats { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string SourceNote { get; set; } = "";
    public override string ToString() => Name;
}
