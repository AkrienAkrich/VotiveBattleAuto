namespace VotiveBattleAuto.Models;

public sealed class BattleLogEntry
{
    public int Round { get; set; }
    public string Actor { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime Time { get; set; } = DateTime.Now;
    public override string ToString() => $"[Круг {Round}] {Text}";
}
