namespace VotiveBattleAuto.Models;

public enum DiceKind
{
    PhysicalAttack,
    PhysicalDefense,
    PhysicalEscape,
    MagicAttack,
    MagicDefense,
    MagicEscape
}

public enum BattleActionType
{
    PhysicalAttack,
    MagicAttack,
    Escape,
    Ability,
    Skip
}

public enum AbilityType
{
    Passive,
    PhysicalAttack,
    PhysicalDefense,
    MagicAttack,
    MagicDefense,
    Control,
    MassAttack,
    MassDefense,
    MassControl,
    Escape,
    Form,
    Healing,
    Support,
    Special
}

public enum TargetMode
{
    Self,
    SingleEnemy,
    SingleAlly,
    MultipleEnemies,
    MultipleAllies,
    AnySingle,
    AnyMultiple
}
