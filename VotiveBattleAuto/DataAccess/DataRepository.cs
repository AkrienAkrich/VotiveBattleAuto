using System.Text.Json;
using System.Text.Json.Serialization;
using VotiveBattleAuto.Models;

namespace VotiveBattleAuto.DataAccess;

public sealed class DataRepository
{
    public string BaseDir { get; }
    public string DataDir { get; }
    public string LogsDir { get; }

    private readonly string _dataDir;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<RaceBonus> Races { get; private set; } = new();
    public List<RoleBonus> Roles { get; private set; } = new();
    public List<Ability> Abilities { get; private set; } = new();

    public DataRepository(string? baseDir = null)
    {
        BaseDir = baseDir ?? AppContext.BaseDirectory;
        DataDir = Path.Combine(BaseDir, "Data");
        LogsDir = Path.Combine(BaseDir, "Logs");
        _dataDir = DataDir;
    }

    public void LoadAll()
    {
        EnsureRuntimeFolders();
        Races = LoadOrCreate("races.json", SeedData.CreateRaces());
        Roles = LoadOrCreate("roles.json", SeedData.CreateRoles());
        Abilities = LoadOrCreate("abilities.json", SeedData.CreateAbilities());
    }

    public void SaveAll()
    {
        EnsureRuntimeFolders();
        Save("races.json", Races);
        Save("roles.json", Roles);
        Save("abilities.json", Abilities);
    }

    public void EnsureRuntimeFolders()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(LogsDir);
    }

    public string SaveBattleLog(IEnumerable<string> lines, string? prefix = null)
    {
        EnsureRuntimeFolders();
        var safePrefix = string.IsNullOrWhiteSpace(prefix) ? "battle" : SanitizeFileName(prefix);
        var fileName = $"{safePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var path = Path.Combine(LogsDir, fileName);
        File.WriteAllLines(path, lines);
        return path;
    }

    public List<string> GetLogFiles()
    {
        EnsureRuntimeFolders();
        return Directory.GetFiles(LogsDir, "*.txt")
            .OrderByDescending(File.GetLastWriteTime)
            .ToList();
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');
        return value.Trim();
    }

    private List<T> LoadOrCreate<T>(string fileName, List<T> seed)
    {
        string path = Path.Combine(_dataDir, fileName);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, JsonSerializer.Serialize(seed, _options));
            return seed;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? seed;
        }
        catch
        {
            string backup = path + ".broken." + DateTime.Now.ToString("yyyyMMddHHmmss");
            File.Copy(path, backup, true);
            File.WriteAllText(path, JsonSerializer.Serialize(seed, _options));
            return seed;
        }
    }

    private void Save<T>(string fileName, List<T> data)
    {
        string path = Path.Combine(_dataDir, fileName);
        File.WriteAllText(path, JsonSerializer.Serialize(data, _options));
    }
}
