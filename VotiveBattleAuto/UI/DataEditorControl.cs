using System.ComponentModel;
using System.Text;
using VotiveBattleAuto.DataAccess;
using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.UI;

public sealed class DataEditorControl : UserControl
{
    private readonly DataRepository _repo;
    private readonly Action<string> _saveAndRefresh;
    private readonly Action _reloadFromDisk;
    private readonly Action _openDataFolder;

    private ListBox _raceList = null!;
    private ListBox _roleList = null!;
    private ListBox _abilityList = null!;

    private TextBox _raceId = null!;
    private TextBox _raceName = null!;
    private TextBox _raceTags = null!;
    private NumericUpDown _raceAttack = null!;
    private NumericUpDown _raceDefense = null!;
    private NumericUpDown _raceEscape = null!;
    private NumericUpDown _raceMagicAttack = null!;
    private NumericUpDown _raceMagicDefense = null!;
    private NumericUpDown _raceMagicEscape = null!;
    private NumericUpDown _raceInitiative = null!;
    private TextBox _raceNote = null!;

    private TextBox _roleId = null!;
    private TextBox _roleName = null!;
    private TextBox _roleTags = null!;
    private NumericUpDown _roleAttack = null!;
    private NumericUpDown _roleDefense = null!;
    private NumericUpDown _roleEscape = null!;
    private NumericUpDown _roleMagicAttack = null!;
    private NumericUpDown _roleMagicDefense = null!;
    private NumericUpDown _roleMagicEscape = null!;
    private NumericUpDown _roleInitiative = null!;
    private TextBox _roleNote = null!;

    private TextBox _abilityId = null!;
    private TextBox _abilityName = null!;
    private TextBox _abilitySource = null!;
    private ComboBox _abilityType = null!;
    private ComboBox _abilityTargetMode = null!;
    private NumericUpDown _abilityMaxTargets = null!;
    private NumericUpDown _abilityCooldown = null!;
    private NumericUpDown _abilityDuration = null!;
    private NumericUpDown _abilitySaveDifficulty = null!;
    private NumericUpDown _abilityHpChange = null!;
    private NumericUpDown _abilityAttack = null!;
    private NumericUpDown _abilityDefense = null!;
    private NumericUpDown _abilityEscape = null!;
    private NumericUpDown _abilityMagicAttack = null!;
    private NumericUpDown _abilityMagicDefense = null!;
    private NumericUpDown _abilityMagicEscape = null!;
    private NumericUpDown _abilityInitiative = null!;
    private TextBox _abilityCasterTags = null!;
    private TextBox _abilityTargetTags = null!;
    private TextBox _abilityForbiddenTags = null!;
    private CheckBox _abilityRequiresWound = null!;
    private CheckBox _abilityRequiresBlood = null!;
    private CheckBox _abilityRequiresSilver = null!;
    private CheckBox _abilityRequiresForm = null!;
    private TextBox _abilityDescription = null!;

    private TextBox _effectName = null!;
    private NumericUpDown _effectDuration = null!;
    private CheckBox _effectSkipAction = null!;
    private CheckBox _effectNoMagic = null!;
    private CheckBox _effectNoEscape = null!;
    private CheckBox _effectIsForm = null!;
    private NumericUpDown _effectAttack = null!;
    private NumericUpDown _effectDefense = null!;
    private NumericUpDown _effectEscape = null!;
    private NumericUpDown _effectMagicAttack = null!;
    private NumericUpDown _effectMagicDefense = null!;
    private NumericUpDown _effectMagicEscape = null!;
    private NumericUpDown _effectInitiative = null!;
    private TextBox _effectAddedTags = null!;

    public DataEditorControl(DataRepository repo, Action<string> saveAndRefresh, Action reloadFromDisk, Action openDataFolder)
    {
        _repo = repo;
        _saveAndRefresh = saveAndRefresh;
        _reloadFromDisk = reloadFromDisk;
        _openDataFolder = openDataFolder;

        Dock = DockStyle.Fill;
        BuildUi();
        RefreshAllLists();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true
        };

        var saveAll = CreateToolbarButton("Сохранить все JSON", 165);
        saveAll.Click += (_, _) => _saveAndRefresh("Все справочники сохранены.");

        var open = CreateToolbarButton("Открыть папку Data", 160);
        open.Click += (_, _) => _openDataFolder();

        var reload = CreateToolbarButton("Перезагрузить JSON", 165);
        reload.Click += (_, _) =>
        {
            _reloadFromDisk();
            RefreshAllLists();
        };

        var hint = new Label
        {
            AutoSize = true,
            Margin = new Padding(20, 8, 0, 0),
            Text = "Редактор справочников: расы/формы, роли и способности. Данные сохраняются в папку Data рядом с exe."
        };

        toolbar.Controls.Add(saveAll);
        toolbar.Controls.Add(open);
        toolbar.Controls.Add(reload);
        toolbar.Controls.Add(hint);
        root.Controls.Add(toolbar, 0, 0);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildRaceTab());
        tabs.TabPages.Add(BuildRoleTab());
        tabs.TabPages.Add(BuildAbilityTab());
        root.Controls.Add(tabs, 0, 1);
    }

    private TabPage BuildRaceTab()
    {
        var page = new TabPage("Расы / формы");
        var shell = CreateSplitEditorShell(page, out _raceList, "Список рас и форм");
        _raceList.SelectedIndexChanged += (_, _) => LoadRaceToForm();

        var editor = CreateRightEditorPanel(shell.Panel2);
        var table = CreateEditorTable();

        _raceId = AddTextRow(table, "Id", "vampire_high");
        _raceName = AddTextRow(table, "Название", "Вампир: высший");
        _raceTags = AddTextRow(table, "Теги", "Vampire, Monster, Undead");
        _raceAttack = AddNumericRow(table, "Атака", -100, 100);
        _raceDefense = AddNumericRow(table, "Защита", -100, 100);
        _raceEscape = AddNumericRow(table, "Побег", -100, 100);
        _raceMagicAttack = AddNumericRow(table, "Маг. атака", -100, 100);
        _raceMagicDefense = AddNumericRow(table, "Маг. защита", -100, 100);
        _raceMagicEscape = AddNumericRow(table, "Маг. побег", -100, 100);
        _raceInitiative = AddNumericRow(table, "Инициатива", -100, 100);
        _raceNote = AddMultilineRow(table, "Заметка", 140);

        editor.Controls.Add(CreateBottomButtons(NewRace, SaveRace, DeleteRace));
        editor.Controls.Add(table);
        return page;
    }

    private TabPage BuildRoleTab()
    {
        var page = new TabPage("Роли");
        var shell = CreateSplitEditorShell(page, out _roleList, "Список ролей");
        _roleList.SelectedIndexChanged += (_, _) => LoadRoleToForm();

        var editor = CreateRightEditorPanel(shell.Panel2);
        var table = CreateEditorTable();

        _roleId = AddTextRow(table, "Id", "hunter");
        _roleName = AddTextRow(table, "Название", "Охотник");
        _roleTags = AddTextRow(table, "Теги", "Hunter");
        _roleAttack = AddNumericRow(table, "Атака", -100, 100);
        _roleDefense = AddNumericRow(table, "Защита", -100, 100);
        _roleEscape = AddNumericRow(table, "Побег", -100, 100);
        _roleMagicAttack = AddNumericRow(table, "Маг. атака", -100, 100);
        _roleMagicDefense = AddNumericRow(table, "Маг. защита", -100, 100);
        _roleMagicEscape = AddNumericRow(table, "Маг. побег", -100, 100);
        _roleInitiative = AddNumericRow(table, "Инициатива", -100, 100);
        _roleNote = AddMultilineRow(table, "Заметка", 140);

        editor.Controls.Add(CreateBottomButtons(NewRole, SaveRole, DeleteRole));
        editor.Controls.Add(table);
        return page;
    }

    private TabPage BuildAbilityTab()
    {
        var page = new TabPage("Способности");
        var split = CreateSplitEditorShell(page, out _abilityList, "Список способностей");
        _abilityList.SelectedIndexChanged += (_, _) => LoadAbilityToForm();

        var leftTop = new Panel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6, 6, 6, 0) };
        var filter = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        filter.Items.AddRange(new object[] { "Все", "Magic", "Hunter", "Lycan", "Vampire", "Custom" });
        filter.SelectedIndex = 0;
        filter.SelectedIndexChanged += (_, _) => RefreshAbilityList(filter.Text);
        leftTop.Controls.Add(filter);
        split.Panel1.Controls.Add(leftTop);
        _abilityList.BringToFront();

        var rightTabs = new TabControl { Dock = DockStyle.Fill };
        split.Panel2.Controls.Add(rightTabs);

        var mainPage = new TabPage("Основные поля");
        var mainEditor = CreateScrollPanel();
        var table = CreateEditorTable();
        mainPage.Controls.Add(mainEditor);
        mainEditor.Controls.Add(CreateBottomButtons(NewAbility, SaveAbility, DeleteAbility, DuplicateAbility));
        mainEditor.Controls.Add(table);

        _abilityId = AddTextRow(table, "Id", "hunter_trap");
        _abilityName = AddTextRow(table, "Название", "Охотничья ловушка");
        _abilitySource = AddTextRow(table, "Источник", "Hunter");
        _abilityType = AddComboRow(table, "Тип", Enum.GetValues(typeof(AbilityType)).Cast<object>().ToArray());
        _abilityTargetMode = AddComboRow(table, "Цели", Enum.GetValues(typeof(TargetMode)).Cast<object>().ToArray());
        _abilityMaxTargets = AddNumericRow(table, "Макс. целей", 1, 20, 1);
        _abilityCooldown = AddNumericRow(table, "Кулдаун", 0, 99);
        _abilityDuration = AddNumericRow(table, "Длительность", -1, 99, 1);
        _abilitySaveDifficulty = AddNumericRow(table, "Порог контроля", 0, 12, 7);
        _abilityHpChange = AddNumericRow(table, "HP +/-", -10, 10);
        _abilityAttack = AddNumericRow(table, "Бонус атаки", -100, 100);
        _abilityDefense = AddNumericRow(table, "Бонус защиты", -100, 100);
        _abilityEscape = AddNumericRow(table, "Бонус побега", -100, 100);
        _abilityMagicAttack = AddNumericRow(table, "Бонус маг. атаки", -100, 100);
        _abilityMagicDefense = AddNumericRow(table, "Бонус маг. защиты", -100, 100);
        _abilityMagicEscape = AddNumericRow(table, "Бонус маг. побега", -100, 100);
        _abilityInitiative = AddNumericRow(table, "Бонус инициативы", -100, 100);
        _abilityCasterTags = AddTextRow(table, "Теги носителя", "Hunter, Lycan");
        _abilityTargetTags = AddTextRow(table, "Теги цели", "Monster, Vampire");
        _abilityForbiddenTags = AddTextRow(table, "Запретные теги цели", "Undead");
        AddCheckRow(table, "Условия", out _abilityRequiresWound, out _abilityRequiresBlood, out _abilityRequiresSilver, out _abilityRequiresForm);
        _abilityDescription = AddMultilineRow(table, "Описание", 160);

        var effectPage = new TabPage("Эффект / форма");
        var effectEditor = CreateScrollPanel();
        var effectTable = CreateEditorTable();
        effectPage.Controls.Add(effectEditor);
        effectEditor.Controls.Add(effectTable);

        _effectName = AddTextRow(effectTable, "Название эффекта", "Контроль");
        _effectDuration = AddNumericRow(effectTable, "Длительность", -1, 99, 1);
        AddEffectCheckRow(effectTable, "Флаги эффекта", out _effectSkipAction, out _effectNoMagic, out _effectNoEscape, out _effectIsForm);
        _effectAttack = AddNumericRow(effectTable, "Эфф. атака", -100, 100);
        _effectDefense = AddNumericRow(effectTable, "Эфф. защита", -100, 100);
        _effectEscape = AddNumericRow(effectTable, "Эфф. побег", -100, 100);
        _effectMagicAttack = AddNumericRow(effectTable, "Эфф. маг. атака", -100, 100);
        _effectMagicDefense = AddNumericRow(effectTable, "Эфф. маг. защита", -100, 100);
        _effectMagicEscape = AddNumericRow(effectTable, "Эфф. маг. побег", -100, 100);
        _effectInitiative = AddNumericRow(effectTable, "Эфф. инициатива", -100, 100);
        _effectAddedTags = AddTextRow(effectTable, "Добавляет теги", "Form, BeastForm");

        rightTabs.TabPages.Add(mainPage);
        rightTabs.TabPages.Add(effectPage);
        return page;
    }

    private SplitContainer CreateSplitEditorShell(TabPage page, out ListBox listBox, string leftTitle)
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 320,
            FixedPanel = FixedPanel.Panel1,
            BorderStyle = BorderStyle.FixedSingle
        };
        page.Controls.Add(split);

        split.Panel1.Padding = new Padding(8);
        split.Panel2.Padding = new Padding(8);

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        split.Panel1.Controls.Add(left);

        left.Controls.Add(new Label
        {
            Text = leftTitle,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(Font, FontStyle.Bold)
        }, 0, 0);

        listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            IntegralHeight = false,
            HorizontalScrollbar = true
        };
        left.Controls.Add(listBox, 0, 1);
        return split;
    }

    private Panel CreateRightEditorPanel(Control parent)
    {
        var panel = CreateScrollPanel();
        parent.Controls.Add(panel);
        return panel;
    }

    private static Panel CreateScrollPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(8)
        };
    }

    private static TableLayoutPanel CreateEditorTable()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 0,
            Padding = new Padding(0, 0, 0, 8)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return table;
    }

    private static TextBox AddTextRow(TableLayoutPanel table, string label, string placeholder = "")
    {
        int row = AddRow(table, label);
        var box = new TextBox { Dock = DockStyle.Top, PlaceholderText = placeholder, Margin = new Padding(0, 3, 0, 3) };
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static TextBox AddMultilineRow(TableLayoutPanel table, string label, int height)
    {
        int row = AddRow(table, label);
        var box = new TextBox
        {
            Dock = DockStyle.Top,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Height = height,
            Margin = new Padding(0, 3, 0, 3)
        };
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static NumericUpDown AddNumericRow(TableLayoutPanel table, string label, int min, int max, int value = 0)
    {
        int row = AddRow(table, label);
        var box = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = Math.Clamp(value, min, max),
            Width = 160,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 3, 0, 3)
        };
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static ComboBox AddComboRow(TableLayoutPanel table, string label, object[] items)
    {
        int row = AddRow(table, label);
        var box = new ComboBox
        {
            Dock = DockStyle.Top,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 3, 0, 3)
        };
        box.Items.AddRange(items);
        if (box.Items.Count > 0)
            box.SelectedIndex = 0;
        table.Controls.Add(box, 1, row);
        return box;
    }

    private static void AddCheckRow(TableLayoutPanel table, string label, out CheckBox wound, out CheckBox blood, out CheckBox silver, out CheckBox form)
    {
        int row = AddRow(table, label);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        wound = new CheckBox { Text = "Нужна рана", AutoSize = true };
        blood = new CheckBox { Text = "Нужна кровь", AutoSize = true };
        silver = new CheckBox { Text = "Серебро", AutoSize = true };
        form = new CheckBox { Text = "Форма", AutoSize = true };
        panel.Controls.AddRange(new Control[] { wound, blood, silver, form });
        table.Controls.Add(panel, 1, row);
    }

    private static void AddEffectCheckRow(TableLayoutPanel table, string label, out CheckBox skip, out CheckBox noMagic, out CheckBox noEscape, out CheckBox isForm)
    {
        int row = AddRow(table, label);
        var panel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
        skip = new CheckBox { Text = "Пропуск действия", AutoSize = true };
        noMagic = new CheckBox { Text = "Запрет магии", AutoSize = true };
        noEscape = new CheckBox { Text = "Запрет побега", AutoSize = true };
        isForm = new CheckBox { Text = "Это форма", AutoSize = true };
        panel.Controls.AddRange(new Control[] { skip, noMagic, noEscape, isForm });
        table.Controls.Add(panel, 1, row);
    }

    private static int AddRow(TableLayoutPanel table, string label)
    {
        int row = table.RowCount;
        table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 7, 10, 7)
        }, 0, row);
        return row;
    }

    private static FlowLayoutPanel CreateBottomButtons(Action create, Action save, Action delete, Action? duplicate = null)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 10, 0, 0)
        };
        var newBtn = new Button { Text = "+ новая запись", AutoSize = true, Height = 32 };
        newBtn.Click += (_, _) => create();
        var saveBtn = new Button { Text = "Сохранить", AutoSize = true, Height = 32 };
        saveBtn.Click += (_, _) => save();
        panel.Controls.Add(newBtn);
        panel.Controls.Add(saveBtn);
        if (duplicate != null)
        {
            var duplicateBtn = new Button { Text = "Дубликат", AutoSize = true, Height = 32 };
            duplicateBtn.Click += (_, _) => duplicate();
            panel.Controls.Add(duplicateBtn);
        }
        var deleteBtn = new Button { Text = "Удалить", AutoSize = true, Height = 32 };
        deleteBtn.Click += (_, _) => delete();
        panel.Controls.Add(deleteBtn);
        return panel;
    }

    private static Button CreateToolbarButton(string text, int width)
    {
        return new Button { Text = text, Width = width, Height = 32, Margin = new Padding(0, 4, 8, 4) };
    }

    private void RefreshAllLists()
    {
        RefreshRaceList();
        RefreshRoleList();
        RefreshAbilityList("Все");
    }

    private void RefreshRaceList(string? selectedId = null)
    {
        if (_raceList == null) return;
        selectedId ??= (_raceList.SelectedItem as RaceBonus)?.Id;
        _raceList.DataSource = null;
        _raceList.DataSource = _repo.Races.OrderBy(x => x.Name).ToList();
        _raceList.DisplayMember = nameof(RaceBonus.Name);
        SelectById(_raceList, selectedId);
    }

    private void RefreshRoleList(string? selectedId = null)
    {
        if (_roleList == null) return;
        selectedId ??= (_roleList.SelectedItem as RoleBonus)?.Id;
        _roleList.DataSource = null;
        _roleList.DataSource = _repo.Roles.OrderBy(x => x.Name).ToList();
        _roleList.DisplayMember = nameof(RoleBonus.Name);
        SelectById(_roleList, selectedId);
    }

    private void RefreshAbilityList(string filter = "Все", string? selectedId = null)
    {
        if (_abilityList == null) return;
        selectedId ??= (_abilityList.SelectedItem as Ability)?.Id;
        IEnumerable<Ability> query = _repo.Abilities.OrderBy(x => x.Source).ThenBy(x => x.Name);
        if (!string.IsNullOrWhiteSpace(filter) && filter != "Все")
            query = query.Where(x => string.Equals(x.Source, filter, StringComparison.OrdinalIgnoreCase));
        _abilityList.DataSource = null;
        _abilityList.DataSource = query.ToList();
        _abilityList.DisplayMember = nameof(Ability.Name);
        SelectById(_abilityList, selectedId);
    }

    private void LoadRaceToForm()
    {
        if (_raceList.SelectedItem is not RaceBonus race) return;
        _raceId.Text = race.Id;
        _raceName.Text = race.Name;
        _raceTags.Text = JoinTags(race.Tags);
        SetStats(race.Stats, _raceAttack, _raceDefense, _raceEscape, _raceMagicAttack, _raceMagicDefense, _raceMagicEscape, _raceInitiative);
        _raceNote.Text = race.SourceNote;
    }

    private void LoadRoleToForm()
    {
        if (_roleList.SelectedItem is not RoleBonus role) return;
        _roleId.Text = role.Id;
        _roleName.Text = role.Name;
        _roleTags.Text = JoinTags(role.Tags);
        SetStats(role.Stats, _roleAttack, _roleDefense, _roleEscape, _roleMagicAttack, _roleMagicDefense, _roleMagicEscape, _roleInitiative);
        _roleNote.Text = role.SourceNote;
    }

    private void LoadAbilityToForm()
    {
        if (_abilityList.SelectedItem is not Ability ability) return;
        _abilityId.Text = ability.Id;
        _abilityName.Text = ability.Name;
        _abilitySource.Text = ability.Source;
        _abilityType.SelectedItem = ability.Type;
        _abilityTargetMode.SelectedItem = ability.TargetMode;
        SafeSet(_abilityMaxTargets, ability.MaxTargets);
        SafeSet(_abilityCooldown, ability.CooldownRounds);
        SafeSet(_abilityDuration, ability.DurationRounds);
        SafeSet(_abilitySaveDifficulty, ability.SaveDifficulty);
        SafeSet(_abilityHpChange, ability.HpChange);
        SetStats(ability.GetInstantBonus(), _abilityAttack, _abilityDefense, _abilityEscape, _abilityMagicAttack, _abilityMagicDefense, _abilityMagicEscape, _abilityInitiative);
        _abilityCasterTags.Text = JoinTags(ability.RequiredCasterTags);
        _abilityTargetTags.Text = JoinTags(ability.RequiredTargetTags);
        _abilityForbiddenTags.Text = JoinTags(ability.ForbiddenTargetTags);
        _abilityRequiresWound.Checked = ability.RequiresWound;
        _abilityRequiresBlood.Checked = ability.RequiresBlood;
        _abilityRequiresSilver.Checked = ability.RequiresSilverWeapon;
        _abilityRequiresForm.Checked = ability.RequiresForm;
        _abilityDescription.Text = ability.Description;

        var effect = ability.AppliedEffect;
        _effectName.Text = effect?.Name ?? "";
        SafeSet(_effectDuration, effect?.DurationRounds ?? 1);
        _effectSkipAction.Checked = effect?.SkipAction ?? false;
        _effectNoMagic.Checked = effect?.CannotUseMagic ?? false;
        _effectNoEscape.Checked = effect?.CannotEscape ?? false;
        _effectIsForm.Checked = effect?.IsForm ?? false;
        SetStats(effect?.StatModifiers ?? new CombatStats(), _effectAttack, _effectDefense, _effectEscape, _effectMagicAttack, _effectMagicDefense, _effectMagicEscape, _effectInitiative);
        _effectAddedTags.Text = effect == null ? "" : JoinTags(effect.AddedTags);
    }

    private void NewRace()
    {
        _raceList.ClearSelected();
        _raceId.Text = "";
        _raceName.Text = "Новая раса / форма";
        _raceTags.Text = "";
        SetStats(new CombatStats(), _raceAttack, _raceDefense, _raceEscape, _raceMagicAttack, _raceMagicDefense, _raceMagicEscape, _raceInitiative);
        _raceNote.Text = "";
    }

    private void NewRole()
    {
        _roleList.ClearSelected();
        _roleId.Text = "";
        _roleName.Text = "Новая роль";
        _roleTags.Text = "";
        SetStats(new CombatStats(), _roleAttack, _roleDefense, _roleEscape, _roleMagicAttack, _roleMagicDefense, _roleMagicEscape, _roleInitiative);
        _roleNote.Text = "";
    }

    private void NewAbility()
    {
        _abilityList.ClearSelected();
        _abilityId.Text = "";
        _abilityName.Text = "Новая способность";
        _abilitySource.Text = "Custom";
        _abilityType.SelectedItem = AbilityType.Special;
        _abilityTargetMode.SelectedItem = TargetMode.SingleEnemy;
        SafeSet(_abilityMaxTargets, 1);
        SafeSet(_abilityCooldown, 0);
        SafeSet(_abilityDuration, 1);
        SafeSet(_abilitySaveDifficulty, 7);
        SafeSet(_abilityHpChange, 0);
        SetStats(new CombatStats(), _abilityAttack, _abilityDefense, _abilityEscape, _abilityMagicAttack, _abilityMagicDefense, _abilityMagicEscape, _abilityInitiative);
        _abilityCasterTags.Text = "";
        _abilityTargetTags.Text = "";
        _abilityForbiddenTags.Text = "";
        _abilityRequiresWound.Checked = false;
        _abilityRequiresBlood.Checked = false;
        _abilityRequiresSilver.Checked = false;
        _abilityRequiresForm.Checked = false;
        _abilityDescription.Text = "";
        _effectName.Text = "";
        SafeSet(_effectDuration, 1);
        _effectSkipAction.Checked = false;
        _effectNoMagic.Checked = false;
        _effectNoEscape.Checked = false;
        _effectIsForm.Checked = false;
        SetStats(new CombatStats(), _effectAttack, _effectDefense, _effectEscape, _effectMagicAttack, _effectMagicDefense, _effectMagicEscape, _effectInitiative);
        _effectAddedTags.Text = "";
    }

    private void SaveRace()
    {
        var race = new RaceBonus
        {
            Id = MakeId(_raceId.Text, _raceName.Text, "race"),
            Name = string.IsNullOrWhiteSpace(_raceName.Text) ? "Новая раса / форма" : _raceName.Text.Trim(),
            Tags = SplitTags(_raceTags.Text),
            Stats = ReadStats(_raceAttack, _raceDefense, _raceEscape, _raceMagicAttack, _raceMagicDefense, _raceMagicEscape, _raceInitiative),
            SourceNote = _raceNote.Text.Trim()
        };
        Upsert(_repo.Races, race, x => x.Id);
        _saveAndRefresh("Раса / форма сохранена.");
        RefreshRaceList(race.Id);
    }

    private void SaveRole()
    {
        var role = new RoleBonus
        {
            Id = MakeId(_roleId.Text, _roleName.Text, "role"),
            Name = string.IsNullOrWhiteSpace(_roleName.Text) ? "Новая роль" : _roleName.Text.Trim(),
            Tags = SplitTags(_roleTags.Text),
            Stats = ReadStats(_roleAttack, _roleDefense, _roleEscape, _roleMagicAttack, _roleMagicDefense, _roleMagicEscape, _roleInitiative),
            SourceNote = _roleNote.Text.Trim()
        };
        Upsert(_repo.Roles, role, x => x.Id);
        _saveAndRefresh("Роль сохранена.");
        RefreshRoleList(role.Id);
    }

    private void SaveAbility()
    {
        var ability = ReadAbilityFromForm();
        Upsert(_repo.Abilities, ability, x => x.Id);
        _saveAndRefresh("Способность сохранена.");
        RefreshAbilityList("Все", ability.Id);
    }

    private void DuplicateAbility()
    {
        if (_abilityList.SelectedItem is not Ability selected) return;
        LoadAbilityToForm();
        _abilityId.Text = selected.Id + "_copy";
        _abilityName.Text = selected.Name + " — копия";
        _abilityList.ClearSelected();
    }

    private void DeleteRace()
    {
        if (_raceList.SelectedItem is not RaceBonus race) return;
        if (!ConfirmDelete(race.Name)) return;
        _repo.Races.RemoveAll(x => string.Equals(x.Id, race.Id, StringComparison.OrdinalIgnoreCase));
        _saveAndRefresh("Раса / форма удалена.");
        RefreshRaceList();
        NewRace();
    }

    private void DeleteRole()
    {
        if (_roleList.SelectedItem is not RoleBonus role) return;
        if (!ConfirmDelete(role.Name)) return;
        _repo.Roles.RemoveAll(x => string.Equals(x.Id, role.Id, StringComparison.OrdinalIgnoreCase));
        _saveAndRefresh("Роль удалена.");
        RefreshRoleList();
        NewRole();
    }

    private void DeleteAbility()
    {
        if (_abilityList.SelectedItem is not Ability ability) return;
        if (!ConfirmDelete(ability.Name)) return;
        _repo.Abilities.RemoveAll(x => string.Equals(x.Id, ability.Id, StringComparison.OrdinalIgnoreCase));
        _saveAndRefresh("Способность удалена.");
        RefreshAbilityList();
        NewAbility();
    }

    private Ability ReadAbilityFromForm()
    {
        var ability = new Ability
        {
            Id = MakeId(_abilityId.Text, _abilityName.Text, "ability"),
            Name = string.IsNullOrWhiteSpace(_abilityName.Text) ? "Новая способность" : _abilityName.Text.Trim(),
            Source = string.IsNullOrWhiteSpace(_abilitySource.Text) ? "Custom" : _abilitySource.Text.Trim(),
            Type = _abilityType.SelectedItem is AbilityType abilityType ? abilityType : AbilityType.Special,
            TargetMode = _abilityTargetMode.SelectedItem is TargetMode targetMode ? targetMode : TargetMode.SingleEnemy,
            MaxTargets = (int)_abilityMaxTargets.Value,
            CooldownRounds = (int)_abilityCooldown.Value,
            DurationRounds = (int)_abilityDuration.Value,
            SaveDifficulty = (int)_abilitySaveDifficulty.Value,
            HpChange = (int)_abilityHpChange.Value,
            AttackBonus = (int)_abilityAttack.Value,
            DefenseBonus = (int)_abilityDefense.Value,
            EscapeBonus = (int)_abilityEscape.Value,
            MagicAttackBonus = (int)_abilityMagicAttack.Value,
            MagicDefenseBonus = (int)_abilityMagicDefense.Value,
            MagicEscapeBonus = (int)_abilityMagicEscape.Value,
            InitiativeBonus = (int)_abilityInitiative.Value,
            RequiredCasterTags = SplitTags(_abilityCasterTags.Text),
            RequiredTargetTags = SplitTags(_abilityTargetTags.Text),
            ForbiddenTargetTags = SplitTags(_abilityForbiddenTags.Text),
            RequiresWound = _abilityRequiresWound.Checked,
            RequiresBlood = _abilityRequiresBlood.Checked,
            RequiresSilverWeapon = _abilityRequiresSilver.Checked,
            RequiresForm = _abilityRequiresForm.Checked,
            Description = _abilityDescription.Text.Trim()
        };

        bool hasEffect = !string.IsNullOrWhiteSpace(_effectName.Text)
            || _effectSkipAction.Checked
            || _effectNoMagic.Checked
            || _effectNoEscape.Checked
            || _effectIsForm.Checked
            || _effectAttack.Value != 0
            || _effectDefense.Value != 0
            || _effectEscape.Value != 0
            || _effectMagicAttack.Value != 0
            || _effectMagicDefense.Value != 0
            || _effectMagicEscape.Value != 0
            || _effectInitiative.Value != 0
            || !string.IsNullOrWhiteSpace(_effectAddedTags.Text);

        if (hasEffect)
        {
            ability.AppliedEffect = new StatusEffect
            {
                Name = string.IsNullOrWhiteSpace(_effectName.Text) ? ability.Name : _effectName.Text.Trim(),
                SourceAbilityId = ability.Id,
                DurationRounds = (int)_effectDuration.Value,
                SkipAction = _effectSkipAction.Checked,
                CannotUseMagic = _effectNoMagic.Checked,
                CannotEscape = _effectNoEscape.Checked,
                IsForm = _effectIsForm.Checked,
                StatModifiers = ReadStats(_effectAttack, _effectDefense, _effectEscape, _effectMagicAttack, _effectMagicDefense, _effectMagicEscape, _effectInitiative),
                AddedTags = SplitTags(_effectAddedTags.Text)
            };
        }

        return ability;
    }

    private static CombatStats ReadStats(NumericUpDown attack, NumericUpDown defense, NumericUpDown escape, NumericUpDown magicAttack, NumericUpDown magicDefense, NumericUpDown magicEscape, NumericUpDown initiative)
    {
        return new CombatStats
        {
            Attack = (int)attack.Value,
            Defense = (int)defense.Value,
            Escape = (int)escape.Value,
            MagicAttack = (int)magicAttack.Value,
            MagicDefense = (int)magicDefense.Value,
            MagicEscape = (int)magicEscape.Value,
            Initiative = (int)initiative.Value
        };
    }

    private static void SetStats(CombatStats stats, NumericUpDown attack, NumericUpDown defense, NumericUpDown escape, NumericUpDown magicAttack, NumericUpDown magicDefense, NumericUpDown magicEscape, NumericUpDown initiative)
    {
        SafeSet(attack, stats.Attack);
        SafeSet(defense, stats.Defense);
        SafeSet(escape, stats.Escape);
        SafeSet(magicAttack, stats.MagicAttack);
        SafeSet(magicDefense, stats.MagicDefense);
        SafeSet(magicEscape, stats.MagicEscape);
        SafeSet(initiative, stats.Initiative);
    }

    private static void SafeSet(NumericUpDown box, int value)
    {
        if (value < box.Minimum) value = (int)box.Minimum;
        if (value > box.Maximum) value = (int)box.Maximum;
        box.Value = value;
    }

    private static List<string> SplitTags(string raw)
    {
        return raw.Split(new[] { ',', ';', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string JoinTags(IEnumerable<string> tags)
    {
        return string.Join(", ", tags);
    }

    private static string MakeId(string rawId, string name, string prefix)
    {
        var source = !string.IsNullOrWhiteSpace(rawId) ? rawId : $"{prefix}_{name}";
        var sb = new StringBuilder();
        foreach (var ch in source.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
                sb.Append(ch);
            else if (ch is '_' or '-' or ' ')
                sb.Append('_');
        }
        var id = sb.ToString().Trim('_');
        return string.IsNullOrWhiteSpace(id) ? $"{prefix}_{Guid.NewGuid():N}" : id;
    }

    private static void Upsert<T>(List<T> list, T value, Func<T, string> idSelector)
    {
        var id = idSelector(value);
        var index = list.FindIndex(x => string.Equals(idSelector(x), id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            list[index] = value;
        else
            list.Add(value);
    }

    private static void SelectById(ListBox list, string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        for (int i = 0; i < list.Items.Count; i++)
        {
            var itemId = list.Items[i] switch
            {
                RaceBonus race => race.Id,
                RoleBonus role => role.Id,
                Ability ability => ability.Id,
                _ => ""
            };
            if (string.Equals(itemId, id, StringComparison.OrdinalIgnoreCase))
            {
                list.SelectedIndex = i;
                return;
            }
        }
    }

    private static bool ConfirmDelete(string name)
    {
        return MessageBox.Show($"Удалить запись: {name}?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    }
}
