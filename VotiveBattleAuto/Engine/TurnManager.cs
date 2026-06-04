using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.Engine;

public sealed class TurnManager
{
    private readonly DiceService _dice;
    private readonly StatsCalculator _stats;

    public List<Character> TurnOrder { get; private set; } = new();
    public int CurrentIndex { get; private set; }
    public int Round { get; private set; } = 1;
    public Dictionary<string, int> InitiativeRolls { get; } = new();

    public TurnManager(DiceService dice, StatsCalculator stats)
    {
        _dice = dice;
        _stats = stats;
    }

    public void BuildInitiative(IEnumerable<Character> characters)
    {
        InitiativeRolls.Clear();
        TurnOrder = characters
            .Where(x => x.IsAlive)
            .Select(x => new { Character = x, Value = _dice.D100() + _stats.GetTotalStats(x).Initiative })
            .OrderByDescending(x => x.Value)
            .Select(x =>
            {
                InitiativeRolls[x.Character.Id] = x.Value;
                return x.Character;
            })
            .ToList();

        CurrentIndex = 0;
        Round = 1;
    }

    public Character? Current => TurnOrder.Count == 0 ? null : TurnOrder[Math.Clamp(CurrentIndex, 0, TurnOrder.Count - 1)];

    public void NextTurn()
    {
        if (TurnOrder.Count == 0) return;

        CurrentIndex++;
        if (CurrentIndex >= TurnOrder.Count)
        {
            CurrentIndex = 0;
            Round++;
            TickRound();
        }

        var guard = 0;
        while (TurnOrder.Count > 0 && !TurnOrder[CurrentIndex].IsAlive && guard < 1000)
        {
            CurrentIndex++;
            if (CurrentIndex >= TurnOrder.Count)
            {
                CurrentIndex = 0;
                Round++;
                TickRound();
            }
            guard++;
        }
    }

    private void TickRound()
    {
        foreach (var character in TurnOrder)
        {
            foreach (var key in character.Cooldowns.Keys.ToList())
            {
                character.Cooldowns[key] = Math.Max(0, character.Cooldowns[key] - 1);
            }

            foreach (var effect in character.StatusEffects.Where(e => e.DurationRounds > 0))
            {
                effect.DurationRounds--;
            }

            character.StatusEffects.RemoveAll(e => e.DurationRounds == 0);
        }
    }
}
