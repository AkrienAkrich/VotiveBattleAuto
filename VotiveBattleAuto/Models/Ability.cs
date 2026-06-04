namespace VotiveBattleAuto.Models;

public sealed class Ability
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Source { get; set; } = "Magic"; // Magic, Hunter, Lycan, Vampire, Equipment, CustomImport
    public AbilityType Type { get; set; } = AbilityType.Special;
    public TargetMode TargetMode { get; set; } = TargetMode.SingleEnemy;
    public string Description { get; set; } = "";
    public int MaxTargets { get; set; } = 1;
    public int CooldownRounds { get; set; }
    public int DurationRounds { get; set; } = 1;
    public int SaveDifficulty { get; set; } = 7;
    public int HpChange { get; set; } // отрицательное значение — урон, положительное — лечение
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int EscapeBonus { get; set; }
    public int MagicAttackBonus { get; set; }
    public int MagicDefenseBonus { get; set; }
    public int MagicEscapeBonus { get; set; }
    public int InitiativeBonus { get; set; }
    public bool RequiresWound { get; set; }
    public bool RequiresBlood { get; set; }
    public bool RequiresSilverWeapon { get; set; }
    public bool RequiresForm { get; set; }
    public List<string> RequiredCasterTags { get; set; } = new();
    public List<string> RequiredTargetTags { get; set; } = new();
    public List<string> ForbiddenTargetTags { get; set; } = new();
    public StatusEffect? AppliedEffect { get; set; }

    public CombatStats GetInstantBonus()
    {
        return new CombatStats
        {
            Attack = AttackBonus,
            Defense = DefenseBonus,
            Escape = EscapeBonus,
            MagicAttack = MagicAttackBonus,
            MagicDefense = MagicDefenseBonus,
            MagicEscape = MagicEscapeBonus,
            Initiative = InitiativeBonus
        };
    }

    public override string ToString() => $"{Name} [{Source}]";
}
