using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.Engine;

public sealed class StatsCalculator
{
    private readonly IReadOnlyList<RaceBonus> _races;
    private readonly IReadOnlyList<RoleBonus> _roles;

    public StatsCalculator(IReadOnlyList<RaceBonus> races, IReadOnlyList<RoleBonus> roles)
    {
        _races = races;
        _roles = roles;
    }

    public CombatStats GetTotalStats(Character character)
    {
        var total = character.ManualStats.Clone();
        var race = _races.FirstOrDefault(x => x.Id == character.RaceId);
        var role = _roles.FirstOrDefault(x => x.Id == character.RoleId);

        if (race != null) total += race.Stats;
        if (role != null) total += role.Stats;

        foreach (var effect in character.StatusEffects)
        {
            total += effect.StatModifiers;
        }

        return total;
    }

    public List<string> GetTotalTags(Character character)
    {
        var tags = new HashSet<string>(character.Tags, StringComparer.OrdinalIgnoreCase);
        var race = _races.FirstOrDefault(x => x.Id == character.RaceId);
        var role = _roles.FirstOrDefault(x => x.Id == character.RoleId);

        if (race != null)
        {
            foreach (var tag in race.Tags) tags.Add(tag);
        }

        if (role != null)
        {
            foreach (var tag in role.Tags) tags.Add(tag);
        }

        foreach (var effect in character.StatusEffects)
        {
            foreach (var tag in effect.AddedTags) tags.Add(tag);
        }

        return tags.ToList();
    }
}
