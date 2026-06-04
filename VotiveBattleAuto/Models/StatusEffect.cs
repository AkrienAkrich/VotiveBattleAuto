namespace VotiveBattleAuto.Models;

public sealed class StatusEffect
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Эффект";
    public string SourceAbilityId { get; set; } = "";
    public int DurationRounds { get; set; } = 1; // -1 значит до конца боя
    public bool SkipAction { get; set; }
    public bool CannotUseMagic { get; set; }
    public bool CannotEscape { get; set; }
    public bool IsForm { get; set; }
    public CombatStats StatModifiers { get; set; } = new();
    public List<string> AddedTags { get; set; } = new();

    public StatusEffect Clone()
    {
        return new StatusEffect
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = Name,
            SourceAbilityId = SourceAbilityId,
            DurationRounds = DurationRounds,
            SkipAction = SkipAction,
            CannotUseMagic = CannotUseMagic,
            CannotEscape = CannotEscape,
            IsForm = IsForm,
            StatModifiers = StatModifiers.Clone(),
            AddedTags = AddedTags.ToList()
        };
    }
}
