using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.DataAccess;

public static class SeedData
{
    public static List<RaceBonus> CreateRaces()
    {
        return new List<RaceBonus>
        {
            new() { Id = "human", Name = "Человек", Stats = new CombatStats(), Tags = new List<string>{"Human"}, SourceNote = "Базовый шаблон." },
            new() { Id = "vampire_new", Name = "Вампир: новообращённый", Stats = All(4), Tags = new List<string>{"Vampire","Monster","Undead"}, SourceNote = "Правила боя: вампиры, новообращённый +4/+4/+4." },
            new() { Id = "vampire_low", Name = "Вампир: низший", Stats = All(7), Tags = new List<string>{"Vampire","Monster","Undead"}, SourceNote = "Правила боя: вампиры, низший +7/+7/+7." },
            new() { Id = "vampire_common", Name = "Вампир: обычный", Stats = All(10), Tags = new List<string>{"Vampire","Monster","Undead"}, SourceNote = "Правила боя: вампиры, обычный +10/+10/+10." },
            new() { Id = "vampire_high", Name = "Вампир: высший", Stats = All(13), Tags = new List<string>{"Vampire","Monster","Undead"}, SourceNote = "Правила боя: вампиры, высший +13/+13/+13." },
            new() { Id = "lycan_homid_cliath", Name = "Оборотень: Хомид/Люпус, Клиат", Stats = All(4), Tags = new List<string>{"Lycan","Monster"}, SourceNote = "Правила боя: оборотни Хомид/Люпус, Клиат +4." },
            new() { Id = "lycan_homid_fostern", Name = "Оборотень: Хомид/Люпус, Фостерн", Stats = All(5), Tags = new List<string>{"Lycan","Monster"}, SourceNote = "Правила боя: оборотни Хомид/Люпус, Фостерн +5." },
            new() { Id = "lycan_homid_adren", Name = "Оборотень: Хомид/Люпус, Адрен", Stats = All(6), Tags = new List<string>{"Lycan","Monster"}, SourceNote = "Правила боя: оборотни Хомид/Люпус, Адрен +6." },
            new() { Id = "lycan_glabro_cliath", Name = "Оборотень: Глабро/Гиспо, Клиат", Stats = All(6), Tags = new List<string>{"Lycan","Monster","LycanForm"}, SourceNote = "Правила боя: оборотни Глабро/Гиспо, Клиат +6." },
            new() { Id = "lycan_glabro_fostern", Name = "Оборотень: Глабро/Гиспо, Фостерн", Stats = All(7), Tags = new List<string>{"Lycan","Monster","LycanForm"}, SourceNote = "Правила боя: оборотни Глабро/Гиспо, Фостерн +7." },
            new() { Id = "lycan_glabro_adren", Name = "Оборотень: Глабро/Гиспо, Адрен", Stats = All(8), Tags = new List<string>{"Lycan","Monster","LycanForm"}, SourceNote = "Правила боя: оборотни Глабро/Гиспо, Адрен +8." },
            new() { Id = "lycan_crinos_cliath", Name = "Оборотень: Кринос, Клиат", Stats = All(7), Tags = new List<string>{"Lycan","Monster","LycanForm","Crinos"}, SourceNote = "Правила боя: оборотни Кринос, Клиат +7." },
            new() { Id = "lycan_crinos_fostern", Name = "Оборотень: Кринос, Фостерн", Stats = All(10), Tags = new List<string>{"Lycan","Monster","LycanForm","Crinos"}, SourceNote = "Правила боя: оборотни Кринос, Фостерн +10." },
            new() { Id = "lycan_crinos_adren", Name = "Оборотень: Кринос, Адрен", Stats = All(13), Tags = new List<string>{"Lycan","Monster","LycanForm","Crinos"}, SourceNote = "Правила боя: оборотни Кринос, Адрен +13." },
            new() { Id = "kinfolk", Name = "Кинфолк", Stats = All(3), Tags = new List<string>{"Kinfolk"}, SourceNote = "Правила боя: кинфолк +3 ко всему." }
        };
    }

    public static List<RoleBonus> CreateRoles()
    {
        return new List<RoleBonus>
        {
            new() { Id = "none", Name = "Без роли", Stats = new CombatStats(), Tags = new List<string>(), SourceNote = "Нет роли." },
            new() { Id = "hunter", Name = "Охотник", Stats = new CombatStats(), Tags = new List<string>{"Hunter"}, SourceNote = "Тег роли для выдачи охотничьих способностей; числовые бонусы заполняются по топику при необходимости." },
            new() { Id = "warrior_self", Name = "Воин: самоучка", Stats = new CombatStats{Attack=1}, Tags = new List<string>{"Warrior"}, SourceNote = "Правила боя: воин самоучка +1/+0/+0." },
            new() { Id = "warrior_graduate", Name = "Воин: выпускник", Stats = new CombatStats{Attack=1,Defense=1,Escape=1}, Tags = new List<string>{"Warrior"}, SourceNote = "Правила боя: воин выпускник +1/+1/+1." },
            new() { Id = "warrior_experienced", Name = "Воин: опытный", Stats = new CombatStats{Attack=2,Defense=2,Escape=1}, Tags = new List<string>{"Warrior","Veteran"}, SourceNote = "Правила боя: воин опытный +2/+2/+1." },
            new() { Id = "mage_student", Name = "Маг: ученик", Stats = MagicAll(4), Tags = new List<string>{"Mage"}, SourceNote = "Магический ранг: ученик +4 к спец. броскам." },
            new() { Id = "mage_apprentice", Name = "Маг: подмастерье", Stats = MagicAll(6), Tags = new List<string>{"Mage"}, SourceNote = "Магический ранг: подмастерье +6." },
            new() { Id = "mage", Name = "Маг", Stats = MagicAll(9), Tags = new List<string>{"Mage"}, SourceNote = "Магический ранг: маг +9." },
            new() { Id = "mage_archmage", Name = "Архимаг", Stats = MagicAll(12), Tags = new List<string>{"Mage"}, SourceNote = "Магический ранг: архимаг +12." },
            new() { Id = "mage_master", Name = "Магистр", Stats = MagicAll(14), Tags = new List<string>{"Mage"}, SourceNote = "Магический ранг: магистр +14." }
        };
    }

    public static List<Ability> CreateAbilities()
    {
        return new List<Ability>
        {
            new() { Id="magic_attack", Name="Атакующее заклинание", Source="Magic", Type=AbilityType.MagicAttack, TargetMode=TargetMode.SingleEnemy, Description="Базовая спец. атака по правилам магии.", HpChange=-1 },
            new() { Id="magic_control", Name="Контролирующее заклинание", Source="Magic", Type=AbilityType.Control, TargetMode=TargetMode.SingleEnemy, Description="Контроль: сначала маг. атака против маг. защиты, затем спас-бросок d12.", SaveDifficulty=7, DurationRounds=1, AppliedEffect=new StatusEffect{Name="Контроль",DurationRounds=1,SkipAction=true} },
            new() { Id="magic_mass_attack", Name="Массовое атакующее заклинание", Source="Magic", Type=AbilityType.MassAttack, TargetMode=TargetMode.MultipleEnemies, MaxTargets=3, CooldownRounds=3, HpChange=-1, Description="Массовая магическая атака по нескольким целям; КД задаётся справочником." },
            new() { Id="magic_mass_control", Name="Массовое контролирующее заклинание", Source="Magic", Type=AbilityType.MassControl, TargetMode=TargetMode.MultipleEnemies, MaxTargets=3, CooldownRounds=3, SaveDifficulty=7, Description="Массовый контроль с отдельными спас-бросками целей.", AppliedEffect=new StatusEffect{Name="Массовый контроль",DurationRounds=1,SkipAction=true} },
            new() { Id="magic_heal", Name="Вспомогательное/исцеляющее заклинание", Source="Magic", Type=AbilityType.Healing, TargetMode=TargetMode.SingleAlly, HpChange=1, CooldownRounds=3, Description="Восстановление 1 HP; точные ограничения редактируются в JSON." },

            new() { Id="hunter_silver_weapon", Name="Серебряное оружие", Source="Hunter", Type=AbilityType.PhysicalAttack, TargetMode=TargetMode.SingleEnemy, AttackBonus=2, HpChange=-1, RequiredCasterTags=new List<string>{"Hunter"}, RequiredTargetTags=new List<string>{"Monster"}, RequiresSilverWeapon=true, Description="Шаблон охотничьей атаки против чудовищ. Значения можно заменить по топику охотников." },
            new() { Id="hunter_trap", Name="Охотничья ловушка", Source="Hunter", Type=AbilityType.Control, TargetMode=TargetMode.SingleEnemy, MagicAttackBonus=0, SaveDifficulty=7, CooldownRounds=3, RequiredCasterTags=new List<string>{"Hunter"}, RequiredTargetTags=new List<string>{"Monster"}, Description="Шаблон контроля охотника: ограничение движения чудовища.", AppliedEffect=new StatusEffect{Name="Ловушка",DurationRounds=1,CannotEscape=true,StatModifiers=new CombatStats{Escape=-3,Defense=-1}} },
            new() { Id="hunter_preparation", Name="Подготовка против чудовища", Source="Hunter", Type=AbilityType.Support, TargetMode=TargetMode.Self, CooldownRounds=3, RequiredCasterTags=new List<string>{"Hunter"}, Description="Подготовительный бафф для следующей атаки/защиты; точные бонусы меняются в JSON.", AppliedEffect=new StatusEffect{Name="Подготовка охотника",DurationRounds=1,StatModifiers=new CombatStats{Attack=2,Defense=1}} },

            new() { Id="lycan_claws", Name="Когти оборотня", Source="Lycan", Type=AbilityType.PhysicalAttack, TargetMode=TargetMode.SingleEnemy, AttackBonus=2, HpChange=-1, RequiredCasterTags=new List<string>{"Lycan"}, Description="Шаблон физической атаки оборотня." },
            new() { Id="lycan_regeneration", Name="Регенерация оборотня", Source="Lycan", Type=AbilityType.Healing, TargetMode=TargetMode.Self, HpChange=1, CooldownRounds=3, RequiredCasterTags=new List<string>{"Lycan"}, Description="Шаблон регенерации. Запреты от серебра/огня можно задать статусами и тегами." },
            new() { Id="lycan_rush", Name="Звериный рывок", Source="Lycan", Type=AbilityType.Control, TargetMode=TargetMode.SingleEnemy, AttackBonus=0, SaveDifficulty=7, CooldownRounds=3, RequiredCasterTags=new List<string>{"Lycan"}, Description="Шаблон сбивания темпа/позиции.", AppliedEffect=new StatusEffect{Name="Сбит с темпа",DurationRounds=1,StatModifiers=new CombatStats{Defense=-2,Escape=-2}} },

            new() { Id="vampire_blood_heal", Name="Кровавое восстановление", Source="Vampire", Type=AbilityType.Healing, TargetMode=TargetMode.Self, HpChange=1, CooldownRounds=3, RequiredCasterTags=new List<string>{"Vampire"}, Description="Шаблон восстановления вампира через кровь." },
            new() { Id="vampire_dominance", Name="Доминирование", Source="Vampire", Type=AbilityType.Control, TargetMode=TargetMode.SingleEnemy, MagicAttackBonus=2, SaveDifficulty=7, CooldownRounds=3, RequiredCasterTags=new List<string>{"Vampire"}, Description="Шаблон вампирского контроля.", AppliedEffect=new StatusEffect{Name="Доминирование",DurationRounds=1,SkipAction=true} },
            new() { Id="vampire_swiftness", Name="Ночная стремительность", Source="Vampire", Type=AbilityType.Form, TargetMode=TargetMode.Self, RequiredCasterTags=new List<string>{"Vampire"}, Description="Шаблон формы/баффа скорости вампира.", AppliedEffect=new StatusEffect{Name="Ночная стремительность",DurationRounds=-1,IsForm=true,StatModifiers=new CombatStats{Attack=1,Defense=1,Escape=2}} }
        };
    }

    private static CombatStats All(int value)
    {
        return new CombatStats { Attack = value, Defense = value, Escape = value };
    }

    private static CombatStats MagicAll(int value)
    {
        return new CombatStats { MagicAttack = value, MagicDefense = value, MagicEscape = value };
    }
}
