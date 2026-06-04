namespace VotiveBattleAuto.Models;

public sealed class CombatStats
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Escape { get; set; }
    public int MagicAttack { get; set; }
    public int MagicDefense { get; set; }
    public int MagicEscape { get; set; }
    public int Initiative { get; set; }

    public int Get(DiceKind kind)
    {
        return kind switch
        {
            DiceKind.PhysicalAttack => Attack,
            DiceKind.PhysicalDefense => Defense,
            DiceKind.PhysicalEscape => Escape,
            DiceKind.MagicAttack => MagicAttack,
            DiceKind.MagicDefense => MagicDefense,
            DiceKind.MagicEscape => MagicEscape,
            _ => 0
        };
    }

    public static CombatStats operator +(CombatStats a, CombatStats b)
    {
        return new CombatStats
        {
            Attack = a.Attack + b.Attack,
            Defense = a.Defense + b.Defense,
            Escape = a.Escape + b.Escape,
            MagicAttack = a.MagicAttack + b.MagicAttack,
            MagicDefense = a.MagicDefense + b.MagicDefense,
            MagicEscape = a.MagicEscape + b.MagicEscape,
            Initiative = a.Initiative + b.Initiative
        };
    }

    public CombatStats Clone()
    {
        return new CombatStats
        {
            Attack = Attack,
            Defense = Defense,
            Escape = Escape,
            MagicAttack = MagicAttack,
            MagicDefense = MagicDefense,
            MagicEscape = MagicEscape,
            Initiative = Initiative
        };
    }
}
