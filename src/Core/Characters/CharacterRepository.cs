using Newtonsoft.Json;

namespace ProjectLiberty.Core.Characters;

public sealed class CharacterRepository
{
    private readonly string _savePath;
    private readonly object _sync = new object();
    private CharacterSaveData _data;

    public CharacterRepository(string savePath)
    {
        if (string.IsNullOrWhiteSpace(savePath)) throw new ArgumentException("Save path cannot be empty.", nameof(savePath));
        _savePath = savePath;
        _data = LoadFromDisk();
    }

    public int Count { get { lock (_sync) return _data.Characters.Count; } }
    public int? ActiveSlot { get { lock (_sync) return _data.ActiveSlot; } }

    public IReadOnlyList<CharacterProfile> GetAll()
    {
        lock (_sync) return _data.Characters.OrderBy(x => x.Slot).Select(Clone).ToArray();
    }

    public CharacterProfile? Get(int slot)
    {
        CharacterProfileRules.ValidateSlot(slot);
        lock (_sync)
        {
            var profile = _data.Characters.FirstOrDefault(x => x.Slot == slot);
            return profile == null ? null : Clone(profile);
        }
    }

    public int FirstFreeSlot()
    {
        lock (_sync)
        {
            for (var slot = 1; slot <= CharacterProfileRules.MaximumCharacters; slot++)
                if (_data.Characters.All(x => x.Slot != slot)) return slot;
        }
        return 0;
    }

    public void Save(CharacterProfile profile, bool makeActive = true)
    {
        if (profile == null) throw new ArgumentNullException(nameof(profile));
        var safe = Clone(profile);
        CharacterProfileRules.Normalize(safe);
        safe.UpdatedUtc = DateTime.UtcNow;

        lock (_sync)
        {
            var existing = _data.Characters.FindIndex(x => x.Slot == safe.Slot);
            if (existing < 0)
            {
                if (_data.Characters.Count >= CharacterProfileRules.MaximumCharacters)
                    throw new InvalidOperationException("The maximum of three characters has already been reached.");
                safe.CreatedUtc = safe.UpdatedUtc;
                _data.Characters.Add(safe);
            }
            else
            {
                safe.CreatedUtc = _data.Characters[existing].CreatedUtc;
                _data.Characters[existing] = safe;
            }
            if (makeActive) _data.ActiveSlot = safe.Slot;
            Persist();
        }
    }

    public void SetActive(int slot)
    {
        CharacterProfileRules.ValidateSlot(slot);
        lock (_sync)
        {
            if (_data.Characters.All(x => x.Slot != slot))
                throw new InvalidOperationException($"Character slot {slot} does not exist.");
            _data.ActiveSlot = slot;
            Persist();
        }
    }

    private CharacterSaveData LoadFromDisk()
    {
        if (!File.Exists(_savePath)) return new CharacterSaveData();
        try
        {
            var loaded = JsonConvert.DeserializeObject<CharacterSaveData>(File.ReadAllText(_savePath)) ?? new CharacterSaveData();
            loaded.Characters ??= new List<CharacterProfile>();
            loaded.Characters = loaded.Characters.GroupBy(x => x.Slot).Select(x => x.First()).Take(3).ToList();
            foreach (var profile in loaded.Characters) CharacterProfileRules.Normalize(profile);
            if (loaded.ActiveSlot.HasValue && loaded.Characters.All(x => x.Slot != loaded.ActiveSlot.Value)) loaded.ActiveSlot = null;
            return loaded;
        }
        catch (Exception)
        {
            return new CharacterSaveData();
        }
    }

    private void Persist()
    {
        var directory = Path.GetDirectoryName(_savePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var temporaryPath = _savePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(_data, Formatting.Indented));
        if (File.Exists(_savePath)) File.Replace(temporaryPath, _savePath, null);
        else File.Move(temporaryPath, _savePath);
    }

    private static CharacterProfile Clone(CharacterProfile profile)
    {
        return JsonConvert.DeserializeObject<CharacterProfile>(JsonConvert.SerializeObject(profile))
               ?? throw new InvalidOperationException("Failed to clone character profile.");
    }
}
