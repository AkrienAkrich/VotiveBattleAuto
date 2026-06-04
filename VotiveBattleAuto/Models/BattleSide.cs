namespace VotiveBattleAuto.Models;

public sealed class BattleSide
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Сторона";
    public List<Character> Members { get; set; } = new();
    public bool HasAliveMembers => Members.Any(x => x.IsAlive);
    public override string ToString() => Name;
}
