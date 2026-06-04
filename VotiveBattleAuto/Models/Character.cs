namespace VotiveBattleAuto.Models;

public sealed class Character
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Персонаж";
    public string SideId { get; set; } = "";
    public string RaceId { get; set; } = "human";
    public string RoleId { get; set; } = "none";
    public int MaxHp { get; set; } = 3;
    public int CurrentHp { get; set; } = 3;
    public CombatStats ManualStats { get; set; } = new();
    public List<string> AbilityIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<StatusEffect> StatusEffects { get; set; } = new();
    public Dictionary<string, int> Cooldowns { get; set; } = new();
    public bool Escaped { get; set; }

    public bool IsAlive => CurrentHp > 0 && !Escaped;

    public override string ToString() => Name;

    public Character CloneForSimulation()
    {
        return new Character
        {
            Id = Id,
            Name = Name,
            SideId = SideId,
            RaceId = RaceId,
            RoleId = RoleId,
            MaxHp = MaxHp,
            CurrentHp = CurrentHp,
            ManualStats = ManualStats.Clone(),
            AbilityIds = AbilityIds.ToList(),
            Tags = Tags.ToList(),
            StatusEffects = StatusEffects.Select(x => x.Clone()).ToList(),
            Cooldowns = Cooldowns.ToDictionary(x => x.Key, x => x.Value),
            Escaped = Escaped
        };
    }
}
