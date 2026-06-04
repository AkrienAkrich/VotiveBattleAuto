using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using VotiveBattleAuto.DataAccess;
using VotiveBattleAuto.Engine;
using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.UI;

public sealed class MainForm : Form
{
    private readonly DataRepository _repo = new();
    private BattleEngine _engine = null!;
    private StatsCalculator _stats = null!;
    private readonly DiceService _dice = new();

    private readonly List<SideEditorState> _sideEditors = new();

    private ListBox _turnOrderList = null!;
    private Label _currentTurnLabel = null!;
    private ComboBox _actionCombo = null!;
    private ComboBox _abilityCombo = null!;
    private CheckedListBox _targetsList = null!;
    private TextBox _battleLog = null!;
    private ListBox _logsList = null!;
    private TextBox _logsText = null!;

    public MainForm()
    {
        Text = "Votive Battle Auto — менеджер РП-боя";
        Width = 1480;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1280, 760);

        InitializeData();
        BuildUi();
        RefreshAll();
    }

    private void InitializeData()
    {
        _repo.LoadAll();
        _stats = new StatsCalculator(_repo.Races, _repo.Roles);
        _engine = new BattleEngine(_dice, _stats, _repo.Abilities);

        _engine.Sides.Add(new BattleSide { Name = "Сторона A" });
        _engine.Sides.Add(new BattleSide { Name = "Сторона B" });
    }

    private void BuildUi()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildSetupTab());
        tabs.TabPages.Add(BuildBattleTab());
        tabs.TabPages.Add(BuildDataTab());
        tabs.TabPages.Add(BuildLogsTab());
        Controls.Add(tabs);
    }

    private TabPage BuildSetupTab()
    {
        var tab = new TabPage("Настройка боя");
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tab.Controls.Add(root);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var info = new Button { Text = "Инфо", Width = 100, Height = 30 };
        info.Click += (_, _) => ShowInfoWindow();
        var reset = new Button { Text = "Сбросить бой", Width = 130, Height = 30 };
        reset.Click += (_, _) => ResetBattleSetup();
        toolbar.Controls.Add(info);
        toolbar.Controls.Add(reset);
        toolbar.Controls.Add(new Label
        {
            AutoSize = true,
            Padding = new Padding(16, 7, 0, 0),
            Text = "Настройка разделена на две стороны. Для каждой стороны отдельно выбирается раса/форма, роль, характеристики, теги и способности."
        });
        root.Controls.Add(toolbar, 0, 0);

        var sides = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        sides.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        sides.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.Controls.Add(sides, 0, 1);

        var editorA = CreateSideEditor(0, "Сторона A");
        var editorB = CreateSideEditor(1, "Сторона B");
        _sideEditors.Add(editorA);
        _sideEditors.Add(editorB);
        sides.Controls.Add(editorA.Container, 0, 0);
        sides.Controls.Add(editorB.Container, 1, 0);

        return tab;
    }

    private SideEditorState CreateSideEditor(int sideIndex, string title)
    {
        var state = new SideEditorState { SideIndex = sideIndex };

        var group = new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        state.Container = group;

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 68));
        group.Controls.Add(main);

        var sideHeader = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        sideHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        sideHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sideHeader.Controls.Add(new Label { Text = "Название", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        state.SideNameText = new TextBox { Dock = DockStyle.Fill, Text = title };
        state.SideNameText.TextChanged += (_, _) =>
        {
            var side = GetSide(state.SideIndex);
            side.Name = string.IsNullOrWhiteSpace(state.SideNameText.Text) ? title : state.SideNameText.Text.Trim();
            group.Text = side.Name;
            RefreshBattleView();
        };
        sideHeader.Controls.Add(state.SideNameText, 1, 0);
        main.Controls.Add(sideHeader, 0, 0);

        state.CharactersList = new ListBox { Dock = DockStyle.Fill, DataSource = state.CharacterBinding, DisplayMember = "Name" };
        state.CharactersList.SelectedIndexChanged += (_, _) => LoadSelectedCharacter(state);
        main.Controls.Add(state.CharactersList, 0, 1);

        var characterButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var add = new Button { Text = "+ участник", Width = 105, Height = 28 };
        add.Click += (_, _) => AddCharacter(state);
        var save = new Button { Text = "Сохранить", Width = 105, Height = 28 };
        save.Click += (_, _) => UpdateCharacter(state);
        var remove = new Button { Text = "Удалить", Width = 95, Height = 28 };
        remove.Click += (_, _) => RemoveCharacter(state);
        var clear = new Button { Text = "Очистить", Width = 95, Height = 28 };
        clear.Click += (_, _) => ClearCharacterForm(state);
        characterButtons.Controls.AddRange(new Control[] { add, save, remove, clear });
        main.Controls.Add(characterButtons, 0, 2);

        var edit = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 11,
            Padding = new Padding(0, 4, 0, 0)
        };
        edit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        edit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        edit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        edit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        edit.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.Controls.Add(edit, 0, 3);

        AddLabel(edit, "Имя", 0, 0);
        state.NameText = new TextBox { Dock = DockStyle.Fill };
        edit.Controls.Add(state.NameText, 1, 0);
        AddLabel(edit, "HP", 2, 0);
        state.HpNumeric = Num(1, 20, 3);
        edit.Controls.Add(state.HpNumeric, 3, 0);

        AddLabel(edit, "Раса/форма", 0, 1);
        state.RaceCombo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = new BindingSource { DataSource = _repo.Races },
            DisplayMember = "Name"
        };
        state.RaceCombo.SelectedIndexChanged += (_, _) => RefreshAbilitiesList(state);
        edit.Controls.Add(state.RaceCombo, 1, 1);
        AddLabel(edit, "Роль", 2, 1);
        state.RoleCombo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DataSource = new BindingSource { DataSource = _repo.Roles },
            DisplayMember = "Name"
        };
        state.RoleCombo.SelectedIndexChanged += (_, _) => RefreshAbilitiesList(state);
        edit.Controls.Add(state.RoleCombo, 3, 1);

        state.Atk = AddNumericStat(edit, "Атака", 0, 2);
        state.Def = AddNumericStat(edit, "Защита", 2, 2);
        state.Esc = AddNumericStat(edit, "Побег", 0, 3);
        state.MAtk = AddNumericStat(edit, "Маг. атака", 2, 3);
        state.MDef = AddNumericStat(edit, "Маг. защита", 0, 4);
        state.MEsc = AddNumericStat(edit, "Маг. побег", 2, 4);
        state.Init = AddNumericStat(edit, "Инициатива", 0, 5);

        AddLabel(edit, "Теги", 0, 6);
        state.TagsText = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Напр.: Hunter, SilverWeapon" };
        state.TagsText.TextChanged += (_, _) => RefreshAbilitiesListIfSuitableFilter(state);
        edit.SetColumnSpan(state.TagsText, 3);
        edit.Controls.Add(state.TagsText, 1, 6);

        AddLabel(edit, "Фильтр", 0, 7);
        state.AbilityFilter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        state.AbilityFilter.Items.AddRange(new object[]
        {
            "Все",
            "Подходящие по тегам",
            "Magic",
            "Hunter",
            "Lycan",
            "Vampire"
        });
        state.AbilityFilter.SelectedIndex = 0;
        state.AbilityFilter.SelectedIndexChanged += (_, _) => RefreshAbilitiesList(state);
        edit.SetColumnSpan(state.AbilityFilter, 3);
        edit.Controls.Add(state.AbilityFilter, 1, 7);

        AddLabel(edit, "Способности", 0, 8);
        state.AbilitiesList = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            IntegralHeight = false,
            HorizontalScrollbar = true
        };
        state.AbilitiesList.ItemCheck += (_, e) =>
        {
            if (e.Index < 0 || e.Index >= state.AbilitiesList.Items.Count) return;
            if (state.AbilitiesList.Items[e.Index] is not Ability ability) return;
            if (e.NewValue == CheckState.Checked) state.SelectedAbilityIds.Add(ability.Id);
            else state.SelectedAbilityIds.Remove(ability.Id);
        };
        edit.SetColumnSpan(state.AbilitiesList, 3);
        edit.SetRowSpan(state.AbilitiesList, 3);
        edit.Controls.Add(state.AbilitiesList, 1, 8);

        RefreshAbilitiesList(state);
        return state;
    }

    private TabPage BuildBattleTab()
    {
        var tab = new TabPage("Бой");
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tab.Controls.Add(root);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var info = new Button { Text = "Инфо", Width = 100, Height = 30 };
        info.Click += (_, _) => ShowInfoWindow();
        var start = new Button { Text = "Начать бой", Width = 120, Height = 30 };
        start.Click += (_, _) => StartBattle();
        var autoTurn = new Button { Text = "Автоход", Width = 105, Height = 30 };
        autoTurn.Click += (_, _) => AutoTurn();
        var autoBattle = new Button { Text = "Автобой", Width = 105, Height = 30 };
        autoBattle.Click += (_, _) => AutoBattle();
        toolbar.Controls.AddRange(new Control[] { info, start, autoTurn, autoBattle });
        root.Controls.Add(toolbar, 0, 0);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        root.Controls.Add(main, 0, 1);

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0, 0, 8, 0) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _currentTurnLabel = new Label { Text = "Текущий ход: —", Dock = DockStyle.Fill, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        left.Controls.Add(_currentTurnLabel, 0, 0);
        _turnOrderList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        left.Controls.Add(_turnOrderList, 0, 1);
        main.Controls.Add(Group("Очередь ходов", left), 0, 0);

        var center = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(0, 0, 8, 0) };
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        center.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        center.Controls.Add(new Label { Text = "Действие", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _actionCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _actionCombo.Items.AddRange(new object[] { "Физическая атака", "Магическая атака", "Побег", "Способность", "Пропуск" });
        _actionCombo.SelectedIndex = 0;
        center.Controls.Add(_actionCombo, 0, 1);

        center.Controls.Add(new Label { Text = "Способность текущего участника", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        _abilityCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        center.Controls.Add(_abilityCombo, 0, 3);

        var executePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var run = new Button { Text = "Выполнить действие", Width = 170, Height = 30 };
        run.Click += (_, _) => ManualAction();
        executePanel.Controls.Add(run);
        center.Controls.Add(executePanel, 0, 4);

        _targetsList = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, HorizontalScrollbar = true };
        center.Controls.Add(Group("Цели", _targetsList), 0, 5);
        main.Controls.Add(Group("Управление ходом", center), 1, 0);

        _battleLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            Font = new Font("Consolas", 10)
        };
        main.Controls.Add(Group("Лог боя", _battleLog), 2, 0);
        return tab;
    }

    private TabPage BuildDataTab()
    {
        var tab = new TabPage("Данные");
        var editor = new DataEditorControl(
            _repo,
            SaveDataAndRefresh,
            ReloadDataFromDisk,
            () => OpenFolder(_repo.DataDir))
        {
            Dock = DockStyle.Fill
        };
        tab.Controls.Add(editor);
        return tab;
    }

    private TabPage BuildRaceEditorTab()
    {
        return BuildRaceRoleEditorTab("Расы / формы", true);
    }

    private TabPage BuildRoleEditorTab()
    {
        return BuildRaceRoleEditorTab("Роли", false);
    }

    private TabPage BuildRaceRoleEditorTab(string title, bool isRace)
    {
        var tab = new TabPage(title);
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 360, FixedPanel = FixedPanel.Panel1 };
        tab.Controls.Add(split);

        var list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        RefreshRaceRoleList(list, isRace);
        split.Panel1.Controls.Add(list);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 12, Padding = new Padding(10) };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 11; i++) right.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        split.Panel2.Controls.Add(right);

        AddLabel(right, "Id", 0, 0);
        var id = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "unique_id" };
        right.Controls.Add(id, 1, 0);
        AddLabel(right, "Название", 2, 0);
        var name = new TextBox { Dock = DockStyle.Fill };
        right.Controls.Add(name, 3, 0);

        AddLabel(right, "Теги", 0, 1);
        var tags = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Hunter, Monster, Vampire" };
        right.SetColumnSpan(tags, 3);
        right.Controls.Add(tags, 1, 1);

        var atk = AddNumericStat(right, "Атака", 0, 2);
        var def = AddNumericStat(right, "Защита", 2, 2);
        var esc = AddNumericStat(right, "Побег", 0, 3);
        var mat = AddNumericStat(right, "Маг. атака", 2, 3);
        var mdf = AddNumericStat(right, "Маг. защита", 0, 4);
        var mes = AddNumericStat(right, "Маг. побег", 2, 4);
        var init = AddNumericStat(right, "Инициатива", 0, 5);

        AddLabel(right, "Заметка", 0, 6);
        var note = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
        right.SetColumnSpan(note, 3);
        right.SetRowSpan(note, 3);
        right.Controls.Add(note, 1, 6);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var create = new Button { Text = "+ новая запись", Width = 125, Height = 30 };
        var save = new Button { Text = "Сохранить", Width = 110, Height = 30 };
        var del = new Button { Text = "Удалить", Width = 100, Height = 30 };
        buttons.Controls.AddRange(new Control[] { create, save, del });
        right.SetColumnSpan(buttons, 4);
        right.Controls.Add(buttons, 0, 9);

        void ClearForm()
        {
            id.Text = "";
            name.Text = "";
            tags.Text = "";
            note.Text = "";
            atk.Value = def.Value = esc.Value = mat.Value = mdf.Value = mes.Value = init.Value = 0;
            list.ClearSelected();
        }

        void LoadSelected()
        {
            if (list.SelectedItem == null) return;
            CombatStats stats;
            List<string> itemTags;
            string sourceNote;
            string itemId;
            string itemName;

            if (isRace && list.SelectedItem is RaceBonus race)
            {
                itemId = race.Id; itemName = race.Name; itemTags = race.Tags; sourceNote = race.SourceNote; stats = race.Stats;
            }
            else if (!isRace && list.SelectedItem is RoleBonus role)
            {
                itemId = role.Id; itemName = role.Name; itemTags = role.Tags; sourceNote = role.SourceNote; stats = role.Stats;
            }
            else return;

            id.Text = itemId;
            name.Text = itemName;
            tags.Text = string.Join(", ", itemTags);
            note.Text = sourceNote;
            atk.Value = ClampNum(atk, stats.Attack);
            def.Value = ClampNum(def, stats.Defense);
            esc.Value = ClampNum(esc, stats.Escape);
            mat.Value = ClampNum(mat, stats.MagicAttack);
            mdf.Value = ClampNum(mdf, stats.MagicDefense);
            mes.Value = ClampNum(mes, stats.MagicEscape);
            init.Value = ClampNum(init, stats.Initiative);
        }

        RaceBonus ReadRace() => new()
        {
            Id = MakeId(id.Text, name.Text, "race"),
            Name = string.IsNullOrWhiteSpace(name.Text) ? "Новая раса/форма" : name.Text.Trim(),
            Tags = SplitTags(tags.Text),
            SourceNote = note.Text.Trim(),
            Stats = new CombatStats
            {
                Attack = (int)atk.Value,
                Defense = (int)def.Value,
                Escape = (int)esc.Value,
                MagicAttack = (int)mat.Value,
                MagicDefense = (int)mdf.Value,
                MagicEscape = (int)mes.Value,
                Initiative = (int)init.Value
            }
        };

        RoleBonus ReadRole() => new()
        {
            Id = MakeId(id.Text, name.Text, "role"),
            Name = string.IsNullOrWhiteSpace(name.Text) ? "Новая роль" : name.Text.Trim(),
            Tags = SplitTags(tags.Text),
            SourceNote = note.Text.Trim(),
            Stats = new CombatStats
            {
                Attack = (int)atk.Value,
                Defense = (int)def.Value,
                Escape = (int)esc.Value,
                MagicAttack = (int)mat.Value,
                MagicDefense = (int)mdf.Value,
                MagicEscape = (int)mes.Value,
                Initiative = (int)init.Value
            }
        };

        list.SelectedIndexChanged += (_, _) => LoadSelected();
        create.Click += (_, _) => ClearForm();
        save.Click += (_, _) =>
        {
            if (isRace)
            {
                var value = ReadRace();
                UpsertById(_repo.Races, value, x => x.Id);
            }
            else
            {
                var value = ReadRole();
                UpsertById(_repo.Roles, value, x => x.Id);
            }
            SaveDataAndRefresh("Запись сохранена.");
            RefreshRaceRoleList(list, isRace);
            SelectListItemById(list, id.Text.Trim());
        };
        del.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(id.Text)) return;
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (isRace) _repo.Races.RemoveAll(x => string.Equals(x.Id, id.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            else _repo.Roles.RemoveAll(x => string.Equals(x.Id, id.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            SaveDataAndRefresh("Запись удалена.");
            RefreshRaceRoleList(list, isRace);
            ClearForm();
        };

        return tab;
    }

    private TabPage BuildAbilityEditorTab()
    {
        var tab = new TabPage("Способности");
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 380, FixedPanel = FixedPanel.Panel1 };
        tab.Controls.Add(split);

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        split.Panel1.Controls.Add(left);

        var sourceFilter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        sourceFilter.Items.AddRange(new object[] { "Все", "Magic", "Hunter", "Lycan", "Vampire", "Equipment", "Custom" });
        sourceFilter.SelectedIndex = 0;
        left.Controls.Add(sourceFilter, 0, 0);
        var list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        left.Controls.Add(list, 0, 1);
        var listButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var refresh = new Button { Text = "Обновить список", Width = 130, Height = 30 };
        var open = new Button { Text = "Открыть JSON", Width = 115, Height = 30 };
        listButtons.Controls.AddRange(new Control[] { refresh, open });
        left.Controls.Add(listButtons, 0, 2);

        var rightTabs = new TabControl { Dock = DockStyle.Fill };
        split.Panel2.Controls.Add(rightTabs);

        var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 16, Padding = new Padding(10) };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 15; i++) main.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var mainPage = new TabPage("Основное");
        mainPage.Controls.Add(main);
        rightTabs.TabPages.Add(mainPage);

        AddLabel(main, "Id", 0, 0);
        var id = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "unique_ability_id" };
        main.Controls.Add(id, 1, 0);
        AddLabel(main, "Название", 2, 0);
        var name = new TextBox { Dock = DockStyle.Fill };
        main.Controls.Add(name, 3, 0);

        AddLabel(main, "Источник", 0, 1);
        var source = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
        source.Items.AddRange(new object[] { "Magic", "Hunter", "Lycan", "Vampire", "Equipment", "Custom" });
        main.Controls.Add(source, 1, 1);
        AddLabel(main, "Тип", 2, 1);
        var type = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = Enum.GetValues(typeof(AbilityType)) };
        main.Controls.Add(type, 3, 1);

        AddLabel(main, "Цели", 0, 2);
        var targetMode = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = Enum.GetValues(typeof(TargetMode)) };
        main.Controls.Add(targetMode, 1, 2);
        AddLabel(main, "Макс. целей", 2, 2);
        var maxTargets = Num(1, 20, 1);
        main.Controls.Add(maxTargets, 3, 2);

        AddLabel(main, "КД", 0, 3);
        var cooldown = Num(0, 99, 0);
        main.Controls.Add(cooldown, 1, 3);
        AddLabel(main, "Длительность", 2, 3);
        var duration = new NumericUpDown { Minimum = -1, Maximum = 99, Value = 1, Dock = DockStyle.Fill };
        main.Controls.Add(duration, 3, 3);

        AddLabel(main, "Порог контроля", 0, 4);
        var saveDifficulty = Num(0, 12, 7);
        main.Controls.Add(saveDifficulty, 1, 4);
        AddLabel(main, "HP +/-", 2, 4);
        var hpChange = new NumericUpDown { Minimum = -10, Maximum = 10, Value = 0, Dock = DockStyle.Fill };
        main.Controls.Add(hpChange, 3, 4);

        var atk = AddNumericStat(main, "Атака", 0, 5);
        var def = AddNumericStat(main, "Защита", 2, 5);
        var esc = AddNumericStat(main, "Побег", 0, 6);
        var mat = AddNumericStat(main, "Маг. атака", 2, 6);
        var mdf = AddNumericStat(main, "Маг. защита", 0, 7);
        var mes = AddNumericStat(main, "Маг. побег", 2, 7);
        var init = AddNumericStat(main, "Инициатива", 0, 8);

        AddLabel(main, "Теги носителя", 0, 9);
        var casterTags = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Hunter, Lycan" };
        main.SetColumnSpan(casterTags, 3);
        main.Controls.Add(casterTags, 1, 9);
        AddLabel(main, "Теги цели", 0, 10);
        var targetTags = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Monster, Vampire" };
        main.SetColumnSpan(targetTags, 3);
        main.Controls.Add(targetTags, 1, 10);
        AddLabel(main, "Запрет цели", 0, 11);
        var forbiddenTags = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Undead" };
        main.SetColumnSpan(forbiddenTags, 3);
        main.Controls.Add(forbiddenTags, 1, 11);

        var reqPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var reqWound = new CheckBox { Text = "Нужна рана", AutoSize = true };
        var reqBlood = new CheckBox { Text = "Нужна кровь", AutoSize = true };
        var reqSilver = new CheckBox { Text = "Серебро", AutoSize = true };
        var reqForm = new CheckBox { Text = "Форма", AutoSize = true };
        reqPanel.Controls.AddRange(new Control[] { reqWound, reqBlood, reqSilver, reqForm });
        main.SetColumnSpan(reqPanel, 4);
        main.Controls.Add(reqPanel, 0, 12);

        AddLabel(main, "Описание", 0, 13);
        var desc = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
        main.SetColumnSpan(desc, 3);
        main.SetRowSpan(desc, 2);
        main.Controls.Add(desc, 1, 13);

        var effectPage = new TabPage("Эффект / форма");
        var effect = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 10, Padding = new Padding(10) };
        effect.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        effect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        effect.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        effect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int i = 0; i < 9; i++) effect.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        effect.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        effectPage.Controls.Add(effect);
        rightTabs.TabPages.Add(effectPage);

        AddLabel(effect, "Название эффекта", 0, 0);
        var effName = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "например: Контроль" };
        effect.SetColumnSpan(effName, 3);
        effect.Controls.Add(effName, 1, 0);
        AddLabel(effect, "Длительность", 0, 1);
        var effDuration = new NumericUpDown { Minimum = -1, Maximum = 99, Value = 1, Dock = DockStyle.Fill };
        effect.Controls.Add(effDuration, 1, 1);
        var skip = new CheckBox { Text = "Пропуск действия", AutoSize = true };
        var noMagic = new CheckBox { Text = "Запрет магии", AutoSize = true };
        var noEscape = new CheckBox { Text = "Запрет побега", AutoSize = true };
        var isForm = new CheckBox { Text = "Это форма", AutoSize = true };
        var flags = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        flags.Controls.AddRange(new Control[] { skip, noMagic, noEscape, isForm });
        effect.SetColumnSpan(flags, 4);
        effect.Controls.Add(flags, 0, 2);

        var eatk = AddNumericStat(effect, "Эфф. атака", 0, 3);
        var edef = AddNumericStat(effect, "Эфф. защита", 2, 3);
        var eesc = AddNumericStat(effect, "Эфф. побег", 0, 4);
        var emat = AddNumericStat(effect, "Эфф. маг. атака", 2, 4);
        var emdf = AddNumericStat(effect, "Эфф. маг. защита", 0, 5);
        var emes = AddNumericStat(effect, "Эфф. маг. побег", 2, 5);
        var einit = AddNumericStat(effect, "Эфф. инициатива", 0, 6);
        AddLabel(effect, "Добавляет теги", 0, 7);
        var addedTags = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Form, BeastForm" };
        effect.SetColumnSpan(addedTags, 3);
        effect.Controls.Add(addedTags, 1, 7);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var create = new Button { Text = "+ новая", Width = 100, Height = 30 };
        var save = new Button { Text = "Сохранить", Width = 110, Height = 30 };
        var copy = new Button { Text = "Дубликат", Width = 100, Height = 30 };
        var del = new Button { Text = "Удалить", Width = 100, Height = 30 };
        bottom.Controls.AddRange(new Control[] { create, save, copy, del });
        main.SetColumnSpan(bottom, 4);
        main.Controls.Add(bottom, 0, 15);

        void RefreshAbilityList()
        {
            var selectedId = (list.SelectedItem as Ability)?.Id;
            var filter = sourceFilter.SelectedItem?.ToString() ?? "Все";
            IEnumerable<Ability> data = _repo.Abilities.OrderBy(x => x.Source).ThenBy(x => x.Name);
            if (filter != "Все") data = data.Where(x => string.Equals(x.Source, filter, StringComparison.OrdinalIgnoreCase));
            list.DataSource = null;
            list.DataSource = data.ToList();
            list.DisplayMember = "Name";
            if (!string.IsNullOrWhiteSpace(selectedId)) SelectListItemById(list, selectedId);
        }

        void ClearForm()
        {
            id.Text = ""; name.Text = ""; source.Text = "Custom"; type.SelectedItem = AbilityType.Special; targetMode.SelectedItem = TargetMode.SingleEnemy;
            maxTargets.Value = 1; cooldown.Value = 0; duration.Value = 1; saveDifficulty.Value = 7; hpChange.Value = 0;
            atk.Value = def.Value = esc.Value = mat.Value = mdf.Value = mes.Value = init.Value = 0;
            casterTags.Text = targetTags.Text = forbiddenTags.Text = desc.Text = "";
            reqWound.Checked = reqBlood.Checked = reqSilver.Checked = reqForm.Checked = false;
            effName.Text = ""; effDuration.Value = 1; skip.Checked = noMagic.Checked = noEscape.Checked = isForm.Checked = false;
            eatk.Value = edef.Value = eesc.Value = emat.Value = emdf.Value = emes.Value = einit.Value = 0;
            addedTags.Text = "";
            list.ClearSelected();
        }

        void LoadAbility()
        {
            if (list.SelectedItem is not Ability a) return;
            id.Text = a.Id; name.Text = a.Name; source.Text = a.Source; type.SelectedItem = a.Type; targetMode.SelectedItem = a.TargetMode;
            maxTargets.Value = ClampNum(maxTargets, a.MaxTargets); cooldown.Value = ClampNum(cooldown, a.CooldownRounds); duration.Value = ClampNum(duration, a.DurationRounds); saveDifficulty.Value = ClampNum(saveDifficulty, a.SaveDifficulty); hpChange.Value = ClampNum(hpChange, a.HpChange);
            atk.Value = ClampNum(atk, a.AttackBonus); def.Value = ClampNum(def, a.DefenseBonus); esc.Value = ClampNum(esc, a.EscapeBonus);
            mat.Value = ClampNum(mat, a.MagicAttackBonus); mdf.Value = ClampNum(mdf, a.MagicDefenseBonus); mes.Value = ClampNum(mes, a.MagicEscapeBonus); init.Value = ClampNum(init, a.InitiativeBonus);
            casterTags.Text = string.Join(", ", a.RequiredCasterTags); targetTags.Text = string.Join(", ", a.RequiredTargetTags); forbiddenTags.Text = string.Join(", ", a.ForbiddenTargetTags); desc.Text = a.Description;
            reqWound.Checked = a.RequiresWound; reqBlood.Checked = a.RequiresBlood; reqSilver.Checked = a.RequiresSilverWeapon; reqForm.Checked = a.RequiresForm;
            var e = a.AppliedEffect;
            effName.Text = e?.Name ?? ""; effDuration.Value = ClampNum(effDuration, e?.DurationRounds ?? 1); skip.Checked = e?.SkipAction ?? false; noMagic.Checked = e?.CannotUseMagic ?? false; noEscape.Checked = e?.CannotEscape ?? false; isForm.Checked = e?.IsForm ?? false;
            eatk.Value = ClampNum(eatk, e?.StatModifiers.Attack ?? 0); edef.Value = ClampNum(edef, e?.StatModifiers.Defense ?? 0); eesc.Value = ClampNum(eesc, e?.StatModifiers.Escape ?? 0);
            emat.Value = ClampNum(emat, e?.StatModifiers.MagicAttack ?? 0); emdf.Value = ClampNum(emdf, e?.StatModifiers.MagicDefense ?? 0); emes.Value = ClampNum(emes, e?.StatModifiers.MagicEscape ?? 0); einit.Value = ClampNum(einit, e?.StatModifiers.Initiative ?? 0);
            addedTags.Text = e == null ? "" : string.Join(", ", e.AddedTags);
        }

        Ability ReadAbility()
        {
            var ability = new Ability
            {
                Id = MakeId(id.Text, name.Text, "ability"),
                Name = string.IsNullOrWhiteSpace(name.Text) ? "Новая способность" : name.Text.Trim(),
                Source = string.IsNullOrWhiteSpace(source.Text) ? "Custom" : source.Text.Trim(),
                Type = type.SelectedItem is AbilityType at ? at : AbilityType.Special,
                TargetMode = targetMode.SelectedItem is TargetMode tm ? tm : TargetMode.SingleEnemy,
                Description = desc.Text.Trim(),
                MaxTargets = (int)maxTargets.Value,
                CooldownRounds = (int)cooldown.Value,
                DurationRounds = (int)duration.Value,
                SaveDifficulty = (int)saveDifficulty.Value,
                HpChange = (int)hpChange.Value,
                AttackBonus = (int)atk.Value,
                DefenseBonus = (int)def.Value,
                EscapeBonus = (int)esc.Value,
                MagicAttackBonus = (int)mat.Value,
                MagicDefenseBonus = (int)mdf.Value,
                MagicEscapeBonus = (int)mes.Value,
                InitiativeBonus = (int)init.Value,
                RequiresWound = reqWound.Checked,
                RequiresBlood = reqBlood.Checked,
                RequiresSilverWeapon = reqSilver.Checked,
                RequiresForm = reqForm.Checked,
                RequiredCasterTags = SplitTags(casterTags.Text),
                RequiredTargetTags = SplitTags(targetTags.Text),
                ForbiddenTargetTags = SplitTags(forbiddenTags.Text)
            };

            var effectHasContent = !string.IsNullOrWhiteSpace(effName.Text) || skip.Checked || noMagic.Checked || noEscape.Checked || isForm.Checked ||
                eatk.Value != 0 || edef.Value != 0 || eesc.Value != 0 || emat.Value != 0 || emdf.Value != 0 || emes.Value != 0 || einit.Value != 0 || !string.IsNullOrWhiteSpace(addedTags.Text);
            if (effectHasContent)
            {
                ability.AppliedEffect = new StatusEffect
                {
                    Name = string.IsNullOrWhiteSpace(effName.Text) ? ability.Name : effName.Text.Trim(),
                    DurationRounds = (int)effDuration.Value,
                    SkipAction = skip.Checked,
                    CannotUseMagic = noMagic.Checked,
                    CannotEscape = noEscape.Checked,
                    IsForm = isForm.Checked,
                    StatModifiers = new CombatStats
                    {
                        Attack = (int)eatk.Value,
                        Defense = (int)edef.Value,
                        Escape = (int)eesc.Value,
                        MagicAttack = (int)emat.Value,
                        MagicDefense = (int)emdf.Value,
                        MagicEscape = (int)emes.Value,
                        Initiative = (int)einit.Value
                    },
                    AddedTags = SplitTags(addedTags.Text)
                };
            }
            return ability;
        }

        sourceFilter.SelectedIndexChanged += (_, _) => RefreshAbilityList();
        refresh.Click += (_, _) => RefreshAbilityList();
        open.Click += (_, _) => OpenFile(Path.Combine(_repo.DataDir, "abilities.json"));
        list.SelectedIndexChanged += (_, _) => LoadAbility();
        create.Click += (_, _) => ClearForm();
        copy.Click += (_, _) =>
        {
            if (list.SelectedItem is not Ability selected) return;
            LoadAbility();
            id.Text = selected.Id + "_copy";
            name.Text = selected.Name + " — копия";
            list.ClearSelected();
        };
        save.Click += (_, _) =>
        {
            var a = ReadAbility();
            UpsertById(_repo.Abilities, a, x => x.Id);
            SaveDataAndRefresh("Способность сохранена.");
            RefreshAbilityList();
            SelectListItemById(list, a.Id);
        };
        del.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(id.Text)) return;
            if (MessageBox.Show("Удалить способность?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            _repo.Abilities.RemoveAll(x => string.Equals(x.Id, id.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            foreach (var character in _engine.AllCharacters)
                character.AbilityIds.RemoveAll(x => string.Equals(x, id.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            SaveDataAndRefresh("Способность удалена.");
            RefreshAbilityList();
            ClearForm();
        };

        RefreshAbilityList();
        return tab;
    }

    private TabPage BuildLogsTab()
    {
        var tab = new TabPage("Логи");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(10) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tab.Controls.Add(root);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var saveCurrent = new Button { Text = "Сохранить текущий лог", Width = 170, Height = 30 };
        saveCurrent.Click += (_, _) => SaveCurrentBattleLog();
        var refresh = new Button { Text = "Обновить", Width = 100, Height = 30 };
        refresh.Click += (_, _) => RefreshLogsTab();
        var openFolder = new Button { Text = "Открыть папку Logs", Width = 150, Height = 30 };
        openFolder.Click += (_, _) => OpenFolder(_repo.LogsDir);
        buttons.Controls.AddRange(new Control[] { saveCurrent, refresh, openFolder });
        root.SetColumnSpan(buttons, 2);
        root.Controls.Add(buttons, 0, 0);

        _logsList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        _logsList.SelectedIndexChanged += (_, _) => LoadSelectedLogFile();
        root.Controls.Add(Group("Файлы логов", _logsList), 0, 1);

        _logsText = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            Font = new Font("Consolas", 10)
        };
        root.Controls.Add(Group("Содержимое", _logsText), 1, 1);
        RefreshLogsTab();
        return tab;
    }

    private void ResetBattleSetup()
    {
        if (MessageBox.Show("Очистить всех участников и вернуть две стороны?", "Сброс боя", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _engine.Sides.Clear();
        _engine.Sides.Add(new BattleSide { Name = "Сторона A" });
        _engine.Sides.Add(new BattleSide { Name = "Сторона B" });
        _engine.Log.Clear();
        _engine.Turns.TurnOrder.Clear();
        foreach (var editor in _sideEditors)
        {
            editor.SelectedAbilityIds.Clear();
            ClearCharacterForm(editor);
        }
        RefreshAll();
    }

    private BattleSide GetSide(int index)
    {
        while (_engine.Sides.Count <= index)
        {
            _engine.Sides.Add(new BattleSide { Name = $"Сторона {(char)('A' + _engine.Sides.Count)}" });
        }
        return _engine.Sides[index];
    }

    private void AddCharacter(SideEditorState state)
    {
        var side = GetSide(state.SideIndex);
        var c = ReadCharacterForm(state);
        c.SideId = side.Id;
        side.Members.Add(c);
        RefreshSideEditor(state);
        RefreshBattleView();
    }

    private void UpdateCharacter(SideEditorState state)
    {
        if (state.CharactersList.SelectedItem is not Character selected)
        {
            MessageBox.Show("Сначала выбери участника в списке этой стороны.");
            return;
        }

        var updated = ReadCharacterForm(state);
        selected.Name = updated.Name;
        selected.RaceId = updated.RaceId;
        selected.RoleId = updated.RoleId;
        selected.MaxHp = updated.MaxHp;
        selected.CurrentHp = Math.Min(selected.CurrentHp <= 0 ? updated.MaxHp : selected.CurrentHp, updated.MaxHp);
        selected.ManualStats = updated.ManualStats;
        selected.AbilityIds = updated.AbilityIds;
        selected.Tags = updated.Tags;
        RefreshSideEditor(state);
        RefreshBattleView();
    }

    private void RemoveCharacter(SideEditorState state)
    {
        if (state.CharactersList.SelectedItem is not Character selected) return;
        GetSide(state.SideIndex).Members.Remove(selected);
        ClearCharacterForm(state);
        RefreshSideEditor(state);
        RefreshBattleView();
    }

    private Character ReadCharacterForm(SideEditorState state)
    {
        var name = string.IsNullOrWhiteSpace(state.NameText.Text) ? "Персонаж" : state.NameText.Text.Trim();
        return new Character
        {
            Name = name,
            RaceId = (state.RaceCombo.SelectedItem as RaceBonus)?.Id ?? _repo.Races.FirstOrDefault()?.Id ?? "human",
            RoleId = (state.RoleCombo.SelectedItem as RoleBonus)?.Id ?? _repo.Roles.FirstOrDefault()?.Id ?? "none",
            MaxHp = (int)state.HpNumeric.Value,
            CurrentHp = (int)state.HpNumeric.Value,
            ManualStats = new CombatStats
            {
                Attack = (int)state.Atk.Value,
                Defense = (int)state.Def.Value,
                Escape = (int)state.Esc.Value,
                MagicAttack = (int)state.MAtk.Value,
                MagicDefense = (int)state.MDef.Value,
                MagicEscape = (int)state.MEsc.Value,
                Initiative = (int)state.Init.Value
            },
            AbilityIds = state.SelectedAbilityIds.ToList(),
            Tags = SplitTags(state.TagsText.Text)
        };
    }

    private void LoadSelectedCharacter(SideEditorState state)
    {
        if (state.CharactersList.SelectedItem is not Character c) return;
        state.NameText.Text = c.Name;
        state.HpNumeric.Value = Math.Clamp(c.MaxHp, (int)state.HpNumeric.Minimum, (int)state.HpNumeric.Maximum);
        state.RaceCombo.SelectedItem = _repo.Races.FirstOrDefault(x => x.Id == c.RaceId) ?? _repo.Races.FirstOrDefault();
        state.RoleCombo.SelectedItem = _repo.Roles.FirstOrDefault(x => x.Id == c.RoleId) ?? _repo.Roles.FirstOrDefault();
        state.Atk.Value = ClampNum(state.Atk, c.ManualStats.Attack);
        state.Def.Value = ClampNum(state.Def, c.ManualStats.Defense);
        state.Esc.Value = ClampNum(state.Esc, c.ManualStats.Escape);
        state.MAtk.Value = ClampNum(state.MAtk, c.ManualStats.MagicAttack);
        state.MDef.Value = ClampNum(state.MDef, c.ManualStats.MagicDefense);
        state.MEsc.Value = ClampNum(state.MEsc, c.ManualStats.MagicEscape);
        state.Init.Value = ClampNum(state.Init, c.ManualStats.Initiative);
        state.TagsText.Text = string.Join(", ", c.Tags);
        state.SelectedAbilityIds = c.AbilityIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        RefreshAbilitiesList(state);
    }

    private void ClearCharacterForm(SideEditorState state)
    {
        state.NameText.Text = "";
        state.HpNumeric.Value = 3;
        if (_repo.Races.Count > 0) state.RaceCombo.SelectedIndex = 0;
        if (_repo.Roles.Count > 0) state.RoleCombo.SelectedIndex = 0;
        state.Atk.Value = 0;
        state.Def.Value = 0;
        state.Esc.Value = 0;
        state.MAtk.Value = 0;
        state.MDef.Value = 0;
        state.MEsc.Value = 0;
        state.Init.Value = 0;
        state.TagsText.Text = "";
        state.SelectedAbilityIds.Clear();
        RefreshAbilitiesList(state);
    }

    private void StartBattle()
    {
        if (_engine.Sides.Any(x => x.Members.Count == 0))
        {
            MessageBox.Show("У каждой стороны должен быть хотя бы один участник.");
            return;
        }

        _engine.StartBattle();
        RefreshBattleView();
    }

    private void ManualAction()
    {
        var actor = _engine.Turns.Current;
        if (actor == null)
        {
            MessageBox.Show("Сначала начните бой.");
            return;
        }

        var action = new BattleAction { ActorId = actor.Id };
        action.TargetIds = _targetsList.CheckedItems.Cast<Character>().Select(x => x.Id).ToList();
        action.Type = _actionCombo.SelectedIndex switch
        {
            0 => BattleActionType.PhysicalAttack,
            1 => BattleActionType.MagicAttack,
            2 => BattleActionType.Escape,
            3 => BattleActionType.Ability,
            _ => BattleActionType.Skip
        };

        if (action.Type == BattleActionType.Ability)
        {
            action.AbilityId = (_abilityCombo.SelectedItem as Ability)?.Id;
        }

        _engine.Resolve(action);
        RefreshBattleView();
        ShowWinnerIfAny();
    }

    private void AutoTurn()
    {
        var action = ChooseAiAction();
        if (action == null)
        {
            MessageBox.Show("Сначала начните бой.");
            return;
        }
        _engine.Resolve(action);
        RefreshBattleView();
        ShowWinnerIfAny();
    }

    private void AutoBattle()
    {
        int guard = 0;
        while (_engine.GetWinner() == null && guard < 500)
        {
            var action = ChooseAiAction();
            if (action == null) break;
            _engine.Resolve(action);
            guard++;
        }
        RefreshBattleView();
        ShowWinnerIfAny();
    }

    private BattleAction? ChooseAiAction()
    {
        var actor = _engine.Turns.Current;
        if (actor == null || !actor.IsAlive) return null;

        var enemies = _engine.Sides
            .Where(x => x.Id != actor.SideId)
            .SelectMany(x => x.Members)
            .Where(x => x.IsAlive)
            .OrderBy(x => x.CurrentHp)
            .ToList();

        if (!enemies.Any()) return new BattleAction { ActorId = actor.Id, Type = BattleActionType.Skip };

        var abilities = actor.AbilityIds
            .Select(id => _repo.Abilities.FirstOrDefault(a => a.Id == id))
            .Where(a => a != null)
            .Cast<Ability>()
            .Where(a => _engine.CanUseAbility(actor, a, out _))
            .ToList();

        var heal = abilities.FirstOrDefault(a => a.Type == AbilityType.Healing && actor.CurrentHp < actor.MaxHp);
        if (heal != null)
            return new BattleAction { ActorId = actor.Id, Type = BattleActionType.Ability, AbilityId = heal.Id, TargetIds = new List<string> { actor.Id } };

        var mass = abilities.FirstOrDefault(a => (a.Type == AbilityType.MassAttack || a.Type == AbilityType.MassControl) && enemies.Count >= 2);
        if (mass != null)
            return new BattleAction { ActorId = actor.Id, Type = BattleActionType.Ability, AbilityId = mass.Id, TargetIds = enemies.Take(mass.MaxTargets).Select(x => x.Id).ToList() };

        var control = abilities.FirstOrDefault(a => a.Type == AbilityType.Control && enemies.Any(e => e.CurrentHp >= 2));
        if (control != null)
            return new BattleAction { ActorId = actor.Id, Type = BattleActionType.Ability, AbilityId = control.Id, TargetIds = new List<string> { enemies.Last().Id } };

        var attackAbility = abilities.FirstOrDefault(a => a.Type is AbilityType.PhysicalAttack or AbilityType.MagicAttack);
        if (attackAbility != null)
            return new BattleAction { ActorId = actor.Id, Type = BattleActionType.Ability, AbilityId = attackAbility.Id, TargetIds = new List<string> { enemies.First().Id } };

        var total = _stats.GetTotalStats(actor);
        return new BattleAction
        {
            ActorId = actor.Id,
            Type = total.MagicAttack > total.Attack ? BattleActionType.MagicAttack : BattleActionType.PhysicalAttack,
            TargetIds = new List<string> { enemies.First().Id }
        };
    }

    private void ShowWinnerIfAny()
    {
        var winner = _engine.GetWinner();
        if (winner != null)
        {
            MessageBox.Show($"Победила сторона: {winner.Name}");
        }
    }

    private void RefreshAll()
    {
        foreach (var editor in _sideEditors)
        {
            var side = GetSide(editor.SideIndex);
            editor.SideNameText.Text = side.Name;
            editor.Container.Text = side.Name;
            RefreshSideEditor(editor);
        }
        RefreshBattleView();
    }

    private void RefreshSideEditor(SideEditorState state)
    {
        if (state.CharactersList == null) return;

        var selectedId = (state.CharactersList.SelectedItem as Character)?.Id;
        var side = GetSide(state.SideIndex);

        state.CharacterBinding.RaiseListChangedEvents = false;
        state.CharacterBinding.Clear();
        foreach (var c in side.Members)
            state.CharacterBinding.Add(c);
        state.CharacterBinding.RaiseListChangedEvents = true;
        state.CharacterBinding.ResetBindings();

        if (state.CharacterBinding.Count == 0)
        {
            state.CharactersList.ClearSelected();
            return;
        }

        int index = 0;
        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            for (int i = 0; i < state.CharacterBinding.Count; i++)
            {
                if (state.CharacterBinding[i].Id == selectedId)
                {
                    index = i;
                    break;
                }
            }
        }
        if (index >= 0 && index < state.CharactersList.Items.Count)
            state.CharactersList.SelectedIndex = index;
    }

    private void RefreshBattleView()
    {
        if (_turnOrderList == null) return;

        _turnOrderList.Items.Clear();
        foreach (var c in _engine.Turns.TurnOrder)
        {
            var sideName = _engine.Sides.FirstOrDefault(s => s.Id == c.SideId)?.Name ?? "?";
            var marker = _engine.Turns.Current?.Id == c.Id ? "▶ " : "  ";
            var stats = _stats.GetTotalStats(c);
            _turnOrderList.Items.Add($"{marker}[{sideName}] {c.Name} — {c.CurrentHp}/{c.MaxHp} HP | АТК {stats.Attack}, ЗАЩ {stats.Defense}, МАТК {stats.MagicAttack}, МЗАЩ {stats.MagicDefense} {(c.Escaped ? "(сбежал)" : "")}");
        }

        var current = _engine.Turns.Current;
        _currentTurnLabel.Text = current == null ? "Текущий ход: —" : $"Круг {_engine.Turns.Round}. Ход: {current.Name}";

        _targetsList.Items.Clear();
        foreach (var c in _engine.AllCharacters.Where(x => x.IsAlive && x.Id != current?.Id))
            _targetsList.Items.Add(c, false);
        _targetsList.DisplayMember = "Name";

        var actorAbilities = current == null
            ? new List<Ability>()
            : current.AbilityIds.Select(id => _repo.Abilities.FirstOrDefault(a => a.Id == id)).Where(a => a != null).Cast<Ability>().ToList();
        _abilityCombo.DataSource = null;
        _abilityCombo.DataSource = actorAbilities;
        _abilityCombo.DisplayMember = "Name";

        _battleLog.Text = string.Join(Environment.NewLine, _engine.Log.Select(x => x.ToString()));
        _battleLog.SelectionStart = _battleLog.TextLength;
        _battleLog.ScrollToCaret();
    }

    private void RefreshAbilitiesListIfSuitableFilter(SideEditorState state)
    {
        if (state.AbilityFilter?.SelectedItem?.ToString() == "Подходящие по тегам")
            RefreshAbilitiesList(state);
    }

    private void RefreshAbilitiesList(SideEditorState state)
    {
        if (state.AbilitiesList == null || state.AbilityFilter == null) return;

        var filter = state.AbilityFilter.SelectedItem?.ToString() ?? "Все";
        var tags = GetEditorTags(state);
        IEnumerable<Ability> abilities = _repo.Abilities.OrderBy(x => x.Source).ThenBy(x => x.Name);

        if (filter == "Подходящие по тегам")
        {
            abilities = abilities.Where(a => a.RequiredCasterTags.All(t => tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
        }
        else if (filter != "Все")
        {
            abilities = abilities.Where(a => string.Equals(a.Source, filter, StringComparison.OrdinalIgnoreCase));
        }

        state.AbilitiesList.BeginUpdate();
        state.AbilitiesList.Items.Clear();
        foreach (var ability in abilities)
        {
            state.AbilitiesList.Items.Add(ability, state.SelectedAbilityIds.Contains(ability.Id));
        }
        state.AbilitiesList.EndUpdate();
    }

    private List<string> GetEditorTags(SideEditorState state)
    {
        var result = new HashSet<string>(SplitTags(state.TagsText.Text), StringComparer.OrdinalIgnoreCase);
        if (state.RaceCombo?.SelectedItem is RaceBonus race)
        {
            foreach (var tag in race.Tags) result.Add(tag);
        }
        if (state.RoleCombo?.SelectedItem is RoleBonus role)
        {
            foreach (var tag in role.Tags) result.Add(tag);
        }
        return result.ToList();
    }

    private static List<string> SplitTags(string input)
    {
        return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void RefreshRaceRoleList(ListBox list, bool isRace)
    {
        var selectedId = GetSelectedItemId(list.SelectedItem);
        list.DataSource = null;
        if (isRace)
        {
            list.DataSource = _repo.Races.OrderBy(x => x.Name).ToList();
        }
        else
        {
            list.DataSource = _repo.Roles.OrderBy(x => x.Name).ToList();
        }
        list.DisplayMember = "Name";
        if (!string.IsNullOrWhiteSpace(selectedId)) SelectListItemById(list, selectedId);
    }

    private static string GetSelectedItemId(object? item)
    {
        return item switch
        {
            RaceBonus race => race.Id,
            RoleBonus role => role.Id,
            Ability ability => ability.Id,
            _ => ""
        };
    }

    private static void SelectListItemById(ListBox list, string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        for (int i = 0; i < list.Items.Count; i++)
        {
            if (string.Equals(GetSelectedItemId(list.Items[i]), id, StringComparison.OrdinalIgnoreCase))
            {
                list.SelectedIndex = i;
                return;
            }
        }
    }

    private static void UpsertById<T>(List<T> list, T value, Func<T, string> idSelector)
    {
        var id = idSelector(value);
        var index = list.FindIndex(x => string.Equals(idSelector(x), id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) list[index] = value;
        else list.Add(value);
    }

    private static string MakeId(string rawId, string name, string prefix)
    {
        var source = !string.IsNullOrWhiteSpace(rawId) ? rawId : $"{prefix}_{name}";
        var sb = new StringBuilder();
        foreach (var ch in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (ch is '_' or '-' or ' ') sb.Append('_');
        }
        var id = sb.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(id) ? $"{prefix}_{Guid.NewGuid():N}" : id;
    }

    private void SaveDataAndRefresh(string message)
    {
        try
        {
            _repo.SaveAll();
            RefreshEditorDataSources();
            RefreshBattleView();
            RefreshLogsTab();
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message, "Данные", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось сохранить данные: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ReloadDataFromDisk()
    {
        if (MessageBox.Show("Перезагрузить JSON из папки Data? Несохранённые изменения в редакторе будут потеряны.", "Перезагрузка", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var sides = _engine.Sides.ToList();
        var log = _engine.Log.ToList();
        _repo.LoadAll();
        _stats = new StatsCalculator(_repo.Races, _repo.Roles);
        _engine = new BattleEngine(_dice, _stats, _repo.Abilities);
        foreach (var side in sides)
            _engine.Sides.Add(side);
        foreach (var entry in log)
            _engine.Log.Add(entry);
        RefreshEditorDataSources();
        RefreshAll();
        MessageBox.Show("JSON перезагружены.", "Данные", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RefreshEditorDataSources()
    {
        foreach (var editor in _sideEditors)
        {
            if (editor.RaceCombo == null || editor.RoleCombo == null) continue;
            var raceId = (editor.RaceCombo.SelectedItem as RaceBonus)?.Id;
            var roleId = (editor.RoleCombo.SelectedItem as RoleBonus)?.Id;

            editor.RaceCombo.DataSource = new BindingSource { DataSource = _repo.Races };
            editor.RaceCombo.DisplayMember = "Name";
            editor.RoleCombo.DataSource = new BindingSource { DataSource = _repo.Roles };
            editor.RoleCombo.DisplayMember = "Name";

            editor.RaceCombo.SelectedItem = _repo.Races.FirstOrDefault(x => x.Id == raceId) ?? _repo.Races.FirstOrDefault();
            editor.RoleCombo.SelectedItem = _repo.Roles.FirstOrDefault(x => x.Id == roleId) ?? _repo.Roles.FirstOrDefault();
            RefreshAbilitiesList(editor);
        }
    }

    private void SaveCurrentBattleLog()
    {
        if (_engine.Log.Count == 0)
        {
            MessageBox.Show("Лог боя пуст.", "Логи", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var lines = _engine.Log.Select(x => x.ToString()).ToList();
        var path = _repo.SaveBattleLog(lines, "votive_battle");
        RefreshLogsTab();
        MessageBox.Show($"Лог сохранён:\n{path}", "Логи", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RefreshLogsTab()
    {
        if (_logsList == null) return;
        var selected = (_logsList.SelectedItem as LogFileItem)?.Path;
        _logsList.Items.Clear();
        foreach (var path in _repo.GetLogFiles())
            _logsList.Items.Add(new LogFileItem(path));
        if (!string.IsNullOrWhiteSpace(selected))
        {
            for (int i = 0; i < _logsList.Items.Count; i++)
            {
                if (_logsList.Items[i] is LogFileItem item && string.Equals(item.Path, selected, StringComparison.OrdinalIgnoreCase))
                {
                    _logsList.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void LoadSelectedLogFile()
    {
        if (_logsText == null || _logsList.SelectedItem is not LogFileItem item) return;
        var path = item.Path;
        try
        {
            _logsText.Text = File.Exists(path) ? File.ReadAllText(path) : "Файл не найден.";
        }
        catch (Exception ex)
        {
            _logsText.Text = "Не удалось прочитать лог: " + ex.Message;
        }
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private static void OpenFile(string path)
    {
        if (!File.Exists(path)) return;
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private void ShowInfoWindow()
    {
        using var form = new Form
        {
            Text = "Инструкция — Votive Battle Auto",
            Width = 1120,
            Height = 820,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = true,
            MinimumSize = new Size(960, 680)
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        form.Controls.Add(root);

        var header = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            Text = "Votive Battle Auto — инструкция по настройке и проведению боя"
        };
        root.Controls.Add(header, 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateInfoQuickStartTab());
        tabs.TabPages.Add(CreateInfoSetupTab());
        tabs.TabPages.Add(CreateInfoCombatTab());
        tabs.TabPages.Add(CreateInfoFormulasTab());
        tabs.TabPages.Add(CreateInfoReferencesTab());
        root.Controls.Add(tabs, 0, 1);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        var close = new Button { Text = "Закрыть", Width = 130, Height = 32 };
        close.Click += (_, _) => form.Close();
        bottom.Controls.Add(close);
        root.Controls.Add(bottom, 0, 2);

        form.ShowDialog(this);
    }

    private static TabPage CreateInfoQuickStartTab()
    {
        return CreateInfoTab(
            "Быстрый старт",
            InfoBlock("Что делает приложение",
                "Автоматизирует РП-бой по логике Votive: стороны, участники, инициатива, атака, защита, побег, магическая атака, магическая защита, контроль, массовые способности, формы, лечение, кулдауны и лог боя.",
                "Стигматы и авторские оккультные системы не используются. Бой строится на справочниках: расы/формы, роли, теги, способности."),
            InfoSteps("Минимальный порядок работы",
                "1. На вкладке «Настройка боя» заполни Сторону A и Сторону B.",
                "2. Для каждого участника выбери расу/форму, роль, HP, ручные бонусы, теги и способности.",
                "3. Нажми «+ участник» в панели нужной стороны.",
                "4. Перейди на вкладку «Бой» и нажми «Начать бой».",
                "5. Веди бой вручную через «Выполнить действие» или используй «Автоход» / «Автобой»."),
            InfoBlock("Что важно помнить",
                "Стороны независимы: способности и параметры выбираются отдельно для каждого участника каждой стороны.",
                "Если способность не видна в фильтре «Подходящие по тегам», значит персонажу не хватает тега расы, роли или ручного тега.",
                "Автобой использует простой ИИ. Он нужен для быстрой проверки баланса, а не для идеального тактического поведения.")
        );
    }

    private static TabPage CreateInfoSetupTab()
    {
        return CreateInfoTab(
            "Настройка сторон",
            InfoBlock("Сторона A и Сторона B",
                "Вкладка «Настройка боя» разделена на две большие панели. В каждой панели создаются участники только этой стороны.",
                "Пример: Сторона A — охотник, маг, ветеран. Сторона B — ликантроп, вампир, воин.",
                "Название стороны можно изменить в поле «Название»."),
            InfoSteps("Создание участника",
                "1. Введи имя персонажа.",
                "2. Выбери расу/форму. Например: человек, оборотень, вампир, звериная форма.",
                "3. Выбери роль. Например: охотник, воин, магический ранг.",
                "4. Укажи HP. По стандарту используется 3 HP.",
                "5. Заполни ручные бонусы, если нужны поправки от снаряжения, обстоятельств или правил.",
                "6. Укажи ручные теги через запятую. Например: Hunter, SilverWeapon.",
                "7. Выбери способности в списке.",
                "8. Нажми «+ участник»."),
            InfoSteps("Изменение участника",
                "1. Выбери персонажа в списке его стороны.",
                "2. Измени нужные поля.",
                "3. Нажми «Сохранить».",
                "4. Если нужно начать с пустой формы, нажми «Очистить».",
                "5. Если персонаж больше не нужен, нажми «Удалить»."),
            InfoTable("Основные поля персонажа", new[]
            {
                new[] { "Раса/форма", "Даёт базовые бонусы и теги. Например, Lycan, Vampire, Monster." },
                new[] { "Роль", "Даёт роль, ранг или архетип. Например, Hunter, Mage, Veteran." },
                new[] { "Ручные бонусы", "Добавляются поверх расы/формы, роли и активных эффектов." },
                new[] { "Теги", "Нужны для условий способностей: Hunter, Lycan, Vampire, SilverWeapon." },
                new[] { "Способности", "Выдаются конкретному участнику. У разных персонажей одной стороны могут быть разные наборы." }
            })
        );
    }

    private static TabPage CreateInfoCombatTab()
    {
        return CreateInfoTab(
            "Проведение боя",
            InfoSteps("Запуск боя",
                "1. Перейди на вкладку «Бой».",
                "2. Нажми «Начать бой».",
                "3. Приложение восстановит HP в пределах максимума, сбросит временные эффекты, бросит инициативу d100 и построит очередь ходов.",
                "4. Текущий ход отображается сверху слева."),
            InfoSteps("Ручной ход",
                "1. Посмотри, чей сейчас ход.",
                "2. Выбери действие: физическая атака, магическая атака, побег, способность или пропуск.",
                "3. Выбери цель или несколько целей в списке целей.",
                "4. Если выбрано действие «Способность», выбери способность текущего участника.",
                "5. Нажми «Выполнить действие». Результат появится в логе боя."),
            InfoBlock("Автоход и автобой",
                "«Автоход» выполняет одно действие за текущего участника.",
                "«Автобой» прогоняет бой до победы одной стороны или до лимита ходов.",
                "Простая логика ИИ: лечиться при низком HP, использовать выгодную массовку, пытаться контролить опасную цель, применять атакующую способность, иначе бить обычной атакой."),
            InfoBlock("Контроль и массовые способности",
                "Контроль проходит в два этапа: сначала магическая атака против магической защиты, затем спас-бросок d12 против порога способности.",
                "Массовые способности выбирают несколько целей. Если выбрано больше целей, чем разрешено MaxTargets, приложение возьмёт только допустимое количество.",
                "Форма накладывает постоянный эффект до конца боя или до снятия эффекта, если такая логика прописана в способности.")
        );
    }

    private static TabPage CreateInfoFormulasTab()
    {
        return CreateInfoTab(
            "Формулы",
            InfoTable("Базовые расчёты", new[]
            {
                new[] { "Физическая атака", "d12 + итоговая атака атакующего против d12 + итоговая защита цели" },
                new[] { "Магическая атака", "d12 + итоговая маг. атака атакующего против d12 + итоговая маг. защита цели" },
                new[] { "Побег", "d12 + итоговый побег против d12 + итоговая атака или контроль преследующего" },
                new[] { "Инициатива", "d100 + итоговая инициатива" },
                new[] { "Ничья", "Если итог атаки и защиты равен, бросок перебрасывается" }
            }),
            InfoTable("Итоговые бонусы", new[]
            {
                new[] { "Итоговая атака", "ручная атака + бонус расы/формы + бонус роли + активные эффекты" },
                new[] { "Итоговая защита", "ручная защита + бонус расы/формы + бонус роли + активные эффекты" },
                new[] { "Итоговая маг. атака", "ручная маг. атака + бонус расы/формы + бонус роли + активные эффекты" },
                new[] { "Итоговая маг. защита", "ручная маг. защита + бонус расы/формы + бонус роли + активные эффекты" },
                new[] { "HP", "обычно 3, но можно менять для тестов баланса" }
            }),
            InfoBlock("Контроль",
                "Шаг 1: контролирующий бросает d12 + маг. атака. Цель бросает d12 + маг. защита.",
                "Шаг 2: если контроль прошёл защиту, цель бросает спас-бросок d12.",
                "Если спас-бросок не выше порога способности, эффект контроля накладывается на цель."),
            InfoBlock("Лог боя",
                "Лог показывает броски, бонусы, итоговые значения, попадание или защиту, потерю HP, применённые эффекты, кулдауны и победу стороны.")
        );
    }

    private static TabPage CreateInfoReferencesTab()
    {
        return CreateInfoTab(
            "Справочники",
            InfoBlock("Где лежат данные",
                "Справочники можно редактировать прямо во вкладке «Данные»: расы/формы, роли и способности.",
                "После сохранения приложение записывает JSON в папку Data рядом с exe.",
                "races.json — расы и формы.",
                "roles.json — роли и магические ранги.",
                "abilities.json — способности магии, охотников, оборотней/ликантропов и вампиров."),
            InfoTable("Поля способности", new[]
            {
                new[] { "Id", "Уникальный код способности. Не должен повторяться." },
                new[] { "Name", "Название, которое видно в приложении." },
                new[] { "Source", "Источник: Magic, Hunter, Lycan, Vampire." },
                new[] { "Type", "Тип: PhysicalAttack, MagicAttack, Control, MassAttack, MassControl, Healing, Form, Support, Special." },
                new[] { "RequiredCasterTags", "Какие теги нужны носителю способности." },
                new[] { "RequiredTargetTags", "Какие теги нужны цели." },
                new[] { "CooldownRounds", "Кулдаун в раундах." },
                new[] { "MaxTargets", "Максимум целей для массовых способностей." }
            }),
            InfoBlock("Как добавлять способности охотников, ликантропов и вампиров",
                "Открой вкладку «Данные» → «Способности» и создай новую запись либо отредактируй существующую.",
                "Также можно вручную редактировать Data/abilities.json.",
                "Охотничья способность обычно требует тег Hunter и может требовать SilverWeapon.",
                "Способность ликантропа обычно требует Lycan или тег формы.",
                "Способность вампира обычно требует Vampire.",
                "После изменения JSON перезапусти приложение."),
            InfoBlock("Логи",
                "Вкладка «Логи» сохраняет текущий лог боя в папку Logs рядом с exe.",
                "Папки Data и Logs создаются автоматически при запуске приложения, если их нет."),
            InfoBlock("Если способность не работает",
                "Проверь, выдана ли она персонажу.",
                "Проверь теги носителя и цели.",
                "Проверь кулдаун.",
                "Проверь, выбрана ли цель.",
                "Проверь, не указано ли ограничение в JSON самой способности.")
        );
    }

    private static TabPage CreateInfoTab(string title, params Control[] blocks)
    {
        var page = new TabPage(title);
        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(12)
        };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = blocks.Length
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].Dock = DockStyle.Top;
            blocks[i].Margin = new Padding(0, 0, 0, 12);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(blocks[i], 0, i);
        }

        scroll.Controls.Add(table);
        page.Controls.Add(scroll);
        return page;
    }

    private static GroupBox InfoBlock(string title, params string[] paragraphs)
    {
        var group = new GroupBox
        {
            Text = title,
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(12)
        };

        var text = string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
        var box = new TextBox
        {
            Dock = DockStyle.Top,
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Control,
            WordWrap = true,
            ScrollBars = ScrollBars.None,
            Font = new Font("Segoe UI", 10),
            Text = text,
            Height = CalculateInfoTextHeight(text, 92)
        };

        group.Controls.Add(box);
        return group;
    }

    private static GroupBox InfoSteps(string title, params string[] steps)
    {
        var formatted = steps.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        return InfoBlock(title, formatted);
    }

    private static GroupBox InfoTable(string title, string[][] rows)
    {
        var group = new GroupBox
        {
            Text = title,
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(12)
        };

        var grid = new DataGridView
        {
            Dock = DockStyle.Top,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            Height = Math.Min(340, 48 + rows.Length * 42)
        };
        grid.Columns.Add("Key", "Поле / действие");
        grid.Columns.Add("Value", "Пояснение / формула");
        grid.Columns[0].FillWeight = 32;
        grid.Columns[1].FillWeight = 68;
        foreach (var row in rows)
        {
            var left = row.Length > 0 ? row[0] : "";
            var right = row.Length > 1 ? row[1] : "";
            grid.Rows.Add(left, right);
        }

        group.Controls.Add(grid);
        return group;
    }

    private static int CalculateInfoTextHeight(string text, int minHeight)
    {
        var explicitLines = text.Split('\n').Length;
        var roughWrappedLines = text.Length / 95;
        var lines = Math.Max(explicitLines, roughWrappedLines) + 1;
        return Math.Clamp(lines * 23, minHeight, 260);
    }

    private static void AddLabel(TableLayoutPanel panel, string text, int col, int row)
    {
        panel.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, col, row);
    }

    private static NumericUpDown Num(int min, int max, int value)
    {
        return new NumericUpDown { Minimum = min, Maximum = max, Value = value, Dock = DockStyle.Fill };
    }

    private static NumericUpDown AddNumericStat(TableLayoutPanel panel, string label, int col, int row)
    {
        AddLabel(panel, label, col, row);
        var n = new NumericUpDown { Minimum = -30, Maximum = 50, Value = 0, Dock = DockStyle.Fill };
        panel.Controls.Add(n, col + 1, row);
        return n;
    }

    private static decimal ClampNum(NumericUpDown n, int value)
    {
        return Math.Clamp(value, (int)n.Minimum, (int)n.Maximum);
    }

    private static GroupBox Group(string title, Control content)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8) };
        content.Dock = DockStyle.Fill;
        group.Controls.Add(content);
        return group;
    }

    private static DataGridView Grid<T>(IEnumerable<T> data)
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
            DataSource = data.ToList()
        };
    }

    private static TabPage Page(string title, Control content)
    {
        var page = new TabPage(title);
        page.Controls.Add(content);
        return page;
    }

    private sealed class LogFileItem
    {
        public string Path { get; }
        public string Name => System.IO.Path.GetFileName(Path);

        public LogFileItem(string path)
        {
            Path = path;
        }

        public override string ToString() => Name;
    }

    private sealed class SideEditorState
    {
        public int SideIndex { get; set; }
        public GroupBox Container { get; set; } = null!;
        public TextBox SideNameText { get; set; } = null!;
        public BindingList<Character> CharacterBinding { get; } = new();
        public ListBox CharactersList { get; set; } = null!;
        public TextBox NameText { get; set; } = null!;
        public NumericUpDown HpNumeric { get; set; } = null!;
        public ComboBox RaceCombo { get; set; } = null!;
        public ComboBox RoleCombo { get; set; } = null!;
        public NumericUpDown Atk { get; set; } = null!;
        public NumericUpDown Def { get; set; } = null!;
        public NumericUpDown Esc { get; set; } = null!;
        public NumericUpDown MAtk { get; set; } = null!;
        public NumericUpDown MDef { get; set; } = null!;
        public NumericUpDown MEsc { get; set; } = null!;
        public NumericUpDown Init { get; set; } = null!;
        public TextBox TagsText { get; set; } = null!;
        public ComboBox AbilityFilter { get; set; } = null!;
        public CheckedListBox AbilitiesList { get; set; } = null!;
        public HashSet<string> SelectedAbilityIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
