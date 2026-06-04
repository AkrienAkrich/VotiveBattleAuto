namespace VotiveBattleAuto.Engine;

public sealed class DiceService
{
    private readonly Random _random = new();

    public int Roll(int sides) => _random.Next(1, sides + 1);
    public int D12() => Roll(12);
    public int D100() => Roll(100);
}
