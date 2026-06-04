using System.Text;
using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.Engine;

public sealed class BattleEngine
{
    private readonly DiceService _dice;
    private readonly StatsCalculator _stats;
    private readonly IReadOnlyList<Ability> _abilities;

    public List<BattleSide> Sides { get; } = new();
    public TurnManager Turns { get; }
    public List<BattleLogEntry> Log { get; } = new();

    public BattleEngine(DiceService dice, StatsCalculator stats, IReadOnlyList<Ability> abilities)
    {
        _dice = dice;
        _stats = stats;
        _abilities = abilities;
        Turns = new TurnManager(dice, stats);
    }

    public IEnumerable<Character> AllCharacters => Sides.SelectMany(x => x.Members);

    public void StartBattle()
    {
        foreach (var character in AllCharacters)
        {
            character.CurrentHp = Math.Clamp(character.CurrentHp <= 0 ? character.MaxHp : character.CurrentHp, 0, character.MaxHp);
            character.Escaped = false;
            character.Cooldowns.Clear();
            character.StatusEffects.RemoveAll(x => !x.IsForm);
        }

        Turns.BuildInitiative(AllCharacters);
        var order = string.Join(" → ", Turns.TurnOrder.Select(x => $"{x.Name} ({Turns.InitiativeRolls[x.Id]})"));
        AddLog($"Бой начат. Инициатива: {order}");
    }

    public BattleSide? GetWinner()
    {
        var alive = Sides.Where(x => x.HasAliveMembers).ToList();
        return alive.Count == 1 ? alive[0] : null;
    }

    public Character? FindCharacter(string id) => AllCharacters.FirstOrDefault(x => x.Id == id);
    public Ability? FindAbility(string id) => _abilities.FirstOrDefault(x => x.Id == id);

    public ActionResult Resolve(BattleAction action)
    {
        var actor = FindCharacter(action.ActorId);
        if (actor == null || !actor.IsAlive)
        {
            return new ActionResult { Text = "Действие невозможно: участник не найден или выбыл." };
        }

        if (actor.StatusEffects.Any(x => x.SkipAction))
        {
            var effectNames = string.Join(", ", actor.StatusEffects.Where(x => x.SkipAction).Select(x => x.Name));
            AddLog($"{actor.Name} пропускает ход из-за эффекта: {effectNames}.", actor.Name);
            Turns.NextTurn();
            return new ActionResult { Text = $"{actor.Name} пропускает ход из-за контроля." };
        }

        ActionResult result;
        switch (action.Type)
        {
            case BattleActionType.PhysicalAttack:
                result = ResolveBasicAttack(actor, action.TargetIds, DiceKind.PhysicalAttack, DiceKind.PhysicalDefense, "физическая атака");
                break;
            case BattleActionType.MagicAttack:
                result = ResolveBasicAttack(actor, action.TargetIds, DiceKind.MagicAttack, DiceKind.MagicDefense, "магическая атака");
                break;
            case BattleActionType.Escape:
                result = ResolveEscape(actor);
                break;
            case BattleActionType.Ability:
                result = ResolveAbility(actor, action);
                break;
            default:
                result = new ActionResult { Text = $"{actor.Name} пропускает ход." };
                AddLog(result.Text, actor.Name);
                break;
        }

        Turns.NextTurn();
        return result;
    }

    private ActionResult ResolveBasicAttack(Character actor, List<string> targetIds, DiceKind attackKind, DiceKind defenseKind, string label)
    {
        var target = targetIds.Select(FindCharacter).FirstOrDefault(x => x != null && x.IsAlive);
        if (target == null)
        {
            return new ActionResult { Text = "Нет допустимой цели." };
        }

        var result = RollContest(actor, target, attackKind, defenseKind, null);
        if (result.Success)
        {
            target.CurrentHp = Math.Max(0, target.CurrentHp - 1);
            result.Damage = 1;
            result.AffectedTargetIds.Add(target.Id);
        }

        result.Text = FormatContest(actor, target, label, result, result.Success ? $"Попадание. {target.Name}: {target.CurrentHp}/{target.MaxHp} HP." : "Защита успешна.");
        AddLog(result.Text, actor.Name);
        return result;
    }

    private ActionResult ResolveEscape(Character actor)
    {
        var opponents = Sides
            .Where(side => side.Id != actor.SideId)
            .SelectMany(side => side.Members)
            .Where(x => x.IsAlive)
            .ToList();

        if (actor.StatusEffects.Any(x => x.CannotEscape))
        {
            var escapeBlockedText = $"{actor.Name} не может сбежать из-за эффекта.";
            AddLog(escapeBlockedText, actor.Name);
            return new ActionResult { Text = escapeBlockedText };
        }

        int actorRoll = _dice.D12();
        int actorBonus = _stats.GetTotalStats(actor).Escape;
        int actorTotal = actorRoll + actorBonus;

        var sb = new StringBuilder();
        sb.Append($"{actor.Name} пытается сбежать: {actorRoll}+{actorBonus}={actorTotal}. ");

        int bestStop = int.MinValue;
        foreach (var opponent in opponents)
        {
            int stopRoll = _dice.D12();
            int stopBonus = _stats.GetTotalStats(opponent).Escape;
            int stopTotal = stopRoll + stopBonus;
            bestStop = Math.Max(bestStop, stopTotal);
            sb.Append($"{opponent.Name} останавливает: {stopRoll}+{stopBonus}={stopTotal}. ");
        }

        bool escaped = actorTotal > bestStop;
        if (escaped) actor.Escaped = true;
        sb.Append(escaped ? "Побег успешен." : "Побег сорван.");
        var text = sb.ToString();
        AddLog(text, actor.Name);
        return new ActionResult { Text = text, Success = escaped };
    }

    private ActionResult ResolveAbility(Character actor, BattleAction action)
    {
        var ability = action.AbilityId == null ? null : FindAbility(action.AbilityId);
        if (ability == null)
        {
            return new ActionResult { Text = "Способность не найдена." };
        }

        var canUse = CanUseAbility(actor, ability, out var reason);
        if (!canUse)
        {
            var text = $"{actor.Name} не может применить {ability.Name}: {reason}";
            AddLog(text, actor.Name);
            return new ActionResult { Text = text };
        }

        var result = ability.Type switch
        {
            AbilityType.PhysicalAttack => ResolveAbilityAttack(actor, action.TargetIds, ability, DiceKind.PhysicalAttack, DiceKind.PhysicalDefense),
            AbilityType.MagicAttack => ResolveAbilityAttack(actor, action.TargetIds, ability, DiceKind.MagicAttack, DiceKind.MagicDefense),
            AbilityType.MassAttack => ResolveMassAttack(actor, action.TargetIds, ability),
            AbilityType.Control => ResolveControl(actor, action.TargetIds, ability, false),
            AbilityType.MassControl => ResolveControl(actor, action.TargetIds, ability, true),
            AbilityType.Healing => ResolveHealing(actor, action.TargetIds, ability),
            AbilityType.Form => ResolveForm(actor, ability),
            AbilityType.Support or AbilityType.Special or AbilityType.PhysicalDefense or AbilityType.MagicDefense or AbilityType.MassDefense => ResolveSupport(actor, action.TargetIds, ability),
            _ => new ActionResult { Text = $"{ability.Name}: тип способности пока не обрабатывается." }
        };

        if (ability.CooldownRounds > 0)
        {
            actor.Cooldowns[ability.Id] = ability.CooldownRounds;
        }

        AddLog(result.Text, actor.Name);
        return result;
    }

    public bool CanUseAbility(Character actor, Ability ability, out string reason)
    {
        reason = "";
        if (!actor.AbilityIds.Contains(ability.Id))
        {
            reason = "способность не выдана персонажу";
            return false;
        }

        if (actor.Cooldowns.TryGetValue(ability.Id, out var cooldown) && cooldown > 0)
        {
            reason = $"кулдаун {cooldown} круг.";
            return false;
        }

        if (actor.StatusEffects.Any(x => x.CannotUseMagic) && IsMagicLike(ability.Type))
        {
            reason = "запрещена магия/спец. действие";
            return false;
        }

        var tags = _stats.GetTotalTags(actor);
        foreach (var tag in ability.RequiredCasterTags)
        {
            if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                reason = $"нужен тег носителя: {tag}";
                return false;
            }
        }

        reason = "можно";
        return true;
    }

    private bool IsMagicLike(AbilityType type)
    {
        return type is AbilityType.MagicAttack or AbilityType.MagicDefense or AbilityType.Control or AbilityType.MassAttack or AbilityType.MassControl or AbilityType.MassDefense or AbilityType.Healing or AbilityType.Support or AbilityType.Special;
    }

    private ActionResult ResolveAbilityAttack(Character actor, List<string> targetIds, Ability ability, DiceKind attackKind, DiceKind defenseKind)
    {
        var target = targetIds.Select(FindCharacter).FirstOrDefault(x => x != null && x.IsAlive);
        if (target == null) return new ActionResult { Text = "Нет допустимой цели." };
        if (!TargetAllowed(actor, target, ability, out var reason)) return new ActionResult { Text = reason };

        var result = RollContest(actor, target, attackKind, defenseKind, ability);
        if (result.Success)
        {
            var hpLoss = ability.HpChange < 0 ? Math.Abs(ability.HpChange) : 1;
            target.CurrentHp = Math.Max(0, target.CurrentHp - hpLoss);
            result.Damage = hpLoss;
            result.AffectedTargetIds.Add(target.Id);
            ApplyAbilityEffect(target, ability);
        }

        result.Text = FormatContest(actor, target, ability.Name, result, result.Success ? $"Попадание. {target.Name}: {target.CurrentHp}/{target.MaxHp} HP." : "Защита успешна.");
        return result;
    }

    private ActionResult ResolveMassAttack(Character actor, List<string> targetIds, Ability ability)
    {
        var result = new ActionResult();
        var targets = targetIds.Select(FindCharacter).Where(x => x != null && x.IsAlive).Cast<Character>().Take(Math.Max(1, ability.MaxTargets)).ToList();
        if (targets.Count == 0) return new ActionResult { Text = "Нет целей для массовой атаки." };

        var sb = new StringBuilder();
        sb.Append($"{actor.Name} применяет {ability.Name}. ");
        foreach (var target in targets)
        {
            if (!TargetAllowed(actor, target, ability, out var reason))
            {
                sb.Append($"{target.Name}: {reason}. ");
                continue;
            }

            var single = RollContest(actor, target, DiceKind.MagicAttack, DiceKind.MagicDefense, ability);
            result.Rolls.AddRange(single.Rolls);
            if (single.Success)
            {
                target.CurrentHp = Math.Max(0, target.CurrentHp - 1);
                result.AffectedTargetIds.Add(target.Id);
                ApplyAbilityEffect(target, ability);
                sb.Append($"{target.Name}: попадание, HP {target.CurrentHp}/{target.MaxHp}. ");
            }
            else
            {
                sb.Append($"{target.Name}: защитился. ");
            }
        }
        result.Text = sb.ToString();
        result.Success = result.AffectedTargetIds.Count > 0;
        return result;
    }

    private ActionResult ResolveControl(Character actor, List<string> targetIds, Ability ability, bool mass)
    {
        var targets = targetIds.Select(FindCharacter).Where(x => x != null && x.IsAlive).Cast<Character>().Take(mass ? Math.Max(1, ability.MaxTargets) : 1).ToList();
        if (targets.Count == 0) return new ActionResult { Text = "Нет целей для контроля." };

        var sb = new StringBuilder();
        var result = new ActionResult();
        sb.Append($"{actor.Name} применяет {ability.Name}. ");

        foreach (var target in targets)
        {
            if (!TargetAllowed(actor, target, ability, out var reason))
            {
                sb.Append($"{target.Name}: {reason}. ");
                continue;
            }

            var contest = RollContest(actor, target, DiceKind.MagicAttack, DiceKind.MagicDefense, ability);
            result.Rolls.AddRange(contest.Rolls);
            if (!contest.Success)
            {
                sb.Append($"{target.Name}: магическая защита успешна. ");
                continue;
            }

            int save = _dice.D12();
            bool resisted = save > ability.SaveDifficulty;
            sb.Append($"{target.Name}: спас-бросок {save} против порога {ability.SaveDifficulty}; ");
            if (resisted)
            {
                sb.Append("вышел из контроля. ");
            }
            else
            {
                var effect = ability.AppliedEffect?.Clone() ?? new StatusEffect { Name = ability.Name, DurationRounds = Math.Max(1, ability.DurationRounds), SkipAction = true, SourceAbilityId = ability.Id };
                if (effect.DurationRounds == 0) effect.DurationRounds = 1;
                target.StatusEffects.Add(effect);
                result.AffectedTargetIds.Add(target.Id);
                sb.Append("контроль сработал. ");
            }
        }

        result.Success = result.AffectedTargetIds.Count > 0;
        result.Text = sb.ToString();
        return result;
    }

    private ActionResult ResolveHealing(Character actor, List<string> targetIds, Ability ability)
    {
        var target = targetIds.Select(FindCharacter).FirstOrDefault(x => x != null && x.IsAlive) ?? actor;
        int heal = Math.Max(1, ability.HpChange > 0 ? ability.HpChange : 1);
        int before = target.CurrentHp;
        target.CurrentHp = Math.Min(target.MaxHp, target.CurrentHp + heal);
        ApplyAbilityEffect(target, ability);
        return new ActionResult
        {
            Text = $"{actor.Name} применяет {ability.Name} на {target.Name}: HP {before} → {target.CurrentHp}/{target.MaxHp}.",
            Success = target.CurrentHp > before,
            AffectedTargetIds = { target.Id }
        };
    }

    private ActionResult ResolveForm(Character actor, Ability ability)
    {
        actor.StatusEffects.RemoveAll(x => x.IsForm && x.SourceAbilityId == ability.Id);
        var effect = ability.AppliedEffect?.Clone() ?? new StatusEffect { Name = ability.Name, DurationRounds = -1, IsForm = true, SourceAbilityId = ability.Id, StatModifiers = ability.GetInstantBonus() };
        effect.IsForm = true;
        effect.SourceAbilityId = ability.Id;
        if (effect.DurationRounds == 0) effect.DurationRounds = -1;
        actor.StatusEffects.Add(effect);
        return new ActionResult { Text = $"{actor.Name} входит в форму: {ability.Name}.", Success = true, AffectedTargetIds = { actor.Id } };
    }

    private ActionResult ResolveSupport(Character actor, List<string> targetIds, Ability ability)
    {
        var targets = targetIds.Select(FindCharacter).Where(x => x != null && x.IsAlive).Cast<Character>().Take(Math.Max(1, ability.MaxTargets)).ToList();
        if (targets.Count == 0) targets.Add(actor);

        foreach (var target in targets)
        {
            ApplyAbilityEffect(target, ability);
        }

        return new ActionResult
        {
            Text = $"{actor.Name} применяет {ability.Name}: эффект наложен на {string.Join(", ", targets.Select(x => x.Name))}.",
            Success = true,
            AffectedTargetIds = targets.Select(x => x.Id).ToList()
        };
    }

    private void ApplyAbilityEffect(Character target, Ability ability)
    {
        if (ability.AppliedEffect != null)
        {
            var effect = ability.AppliedEffect.Clone();
            effect.SourceAbilityId = ability.Id;
            if (effect.DurationRounds == 0) effect.DurationRounds = Math.Max(1, ability.DurationRounds);
            target.StatusEffects.Add(effect);
        }
    }

    private bool TargetAllowed(Character actor, Character target, Ability ability, out string reason)
    {
        reason = "";
        var targetTags = _stats.GetTotalTags(target);
        foreach (var tag in ability.RequiredTargetTags)
        {
            if (!targetTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                reason = $"для {ability.Name} цель должна иметь тег {tag}";
                return false;
            }
        }
        foreach (var tag in ability.ForbiddenTargetTags)
        {
            if (targetTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                reason = $"{ability.Name} не работает по цели с тегом {tag}";
                return false;
            }
        }
        return true;
    }

    private ActionResult RollContest(Character attacker, Character defender, DiceKind attackKind, DiceKind defenseKind, Ability? ability)
    {
        var attackStats = _stats.GetTotalStats(attacker);
        var defenseStats = _stats.GetTotalStats(defender);
        if (ability != null)
        {
            attackStats += ability.GetInstantBonus();
        }

        var output = new ActionResult();
        while (true)
        {
            var roll = new ContestRoll
            {
                AttackerRoll = _dice.D12(),
                DefenderRoll = _dice.D12(),
                AttackerBonus = attackStats.Get(attackKind),
                DefenderBonus = defenseStats.Get(defenseKind)
            };
            output.Rolls.Add(roll);

            if (roll.AttackerTotal == roll.DefenderTotal)
            {
                continue;
            }

            output.Success = roll.AttackerTotal > roll.DefenderTotal;
            return output;
        }
    }

    private static string FormatContest(Character attacker, Character defender, string actionName, ActionResult result, string suffix)
    {
        var last = result.Rolls.Last();
        var rerolls = result.Rolls.Count > 1 ? $" Перекидов: {result.Rolls.Count - 1}." : "";
        return $"{attacker.Name} → {defender.Name}: {actionName}. " +
               $"{last.AttackerRoll}+{last.AttackerBonus}={last.AttackerTotal} против " +
               $"{last.DefenderRoll}+{last.DefenderBonus}={last.DefenderTotal}.{rerolls} {suffix}";
    }

    private void AddLog(string text, string actor = "")
    {
        Log.Add(new BattleLogEntry { Round = Turns.Round, Actor = actor, Text = text });
    }
}
