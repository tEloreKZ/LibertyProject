using System.Diagnostics;
using GTA;
using GTA.UI;
using ProjectLiberty.Core;
using ProjectLiberty.Core.Characters;
using ProjectLiberty.Core.DependencyInjection;
using ProjectLiberty.Abstractions.Localization;
using ProjectLiberty.Abstractions.Logging;
using ProjectLiberty.LibertyScript.Characters;
using ProjectLiberty.LibertyScript.UI;
using GTA.Native;

namespace ProjectLiberty.LibertyScript;

/// <summary>
/// Main script for Project Liberty.
/// </summary>
public class LibertyScript : Script
{
    private ServiceBootstrap? _bootstrap;
    private Application? _application;
    private ServiceContainer? _container;
    private ILogger? _logger;
    private ILocalization? _localization;
    private ILocalizationCatalog? _localizationCatalog;
    private CharacterRepository? _characters;
    private CharacterEditorMenu? _characterEditor;
    private CefUiBridge? _cefUi;
    private CefTestController? _cefTest;
    private string _uiView = "hidden";
    private bool _pendingActiveCharacterApply;
    private bool _uiErrorShown;
    private int _nextUiStateAt;
    private string _lastSentUiView = string.Empty;
    private bool _initialized = false;
    private bool _initializationAttempted;
    private bool _firstTickCompleted;
    private int _suppressPauseUntil;
    private string _baseDirectory = string.Empty;
    private readonly Dictionary<int, (DateTime UpdatedUtc, string DataUri)> _portraitCache = new();

    public LibertyScript()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
        Aborted += OnAborted;
    }

    private void Initialize()
    {
        try
        {
            var baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProjectLiberty");
            _baseDirectory = baseDirectory;

            // Initialize bootstrap
            _bootstrap = new ServiceBootstrap(baseDirectory);

            // Configure services
            _container = _bootstrap.ConfigureServices();
            _logger = _container.GetService<ILogger>();
            _localization = _container.GetService<ILocalization>();
            _localizationCatalog = _container.GetService<ILocalizationCatalog>();

            // Get application
            _application = _container.GetService<Application>();

            if (_application == null)
            {
                Notification.Show("~r~Failed to get application from container");
                return;
            }

            // Initialize application
            _application.Initialize();

            // Run application
            _application.Run();
            _logger?.LogInformation("Managed application startup completed");

            CreateCharacterUi(baseDirectory);
            _logger?.LogInformation("Character and CEF client state initialized");
            _cefTest = new CefTestController();

            _initialized = true;
            _logger?.LogInformation("LibertyScript initialized; CEF browser startup is deferred until F2/F10");

            Notification.Show("~g~Project Liberty~w~ initialized successfully");
            Debug.WriteLine("Project Liberty initialized successfully");
        }
        catch (Exception ex)
        {
            Notification.Show($"~r~Failed to initialize Project Liberty: {ex.Message}");
            Debug.WriteLine($"Initialize error: {ex}");
        }
    }

    private void CreateCharacterUi(string baseDirectory)
    {
        var assetsDirectory = Path.Combine(baseDirectory, "Assets", "UI");
        _characters = new CharacterRepository(Path.Combine(baseDirectory, "Saves", "characters.json"));
        _characterEditor = new CharacterEditorMenu(assetsDirectory, _characters,
            _localization ?? throw new InvalidOperationException("Localization service is unavailable"),
            () => _uiView = "dashboard", CaptureCharacterPortrait);
        _cefUi = new CefUiBridge();
        _pendingActiveCharacterApply = _characters.ActiveSlot.HasValue;
    }

    private void BeginCharacterCreation(int slot)
    {
        if (_characters == null || _characterEditor == null) return;
        if (_characters.Get(slot) != null)
        {
            SelectCharacter(slot);
            return;
        }
        if (_characters.Count >= CharacterProfileRules.MaximumCharacters)
        {
            Notification.Show($"~p~Project Liberty~w~: {T("ui.notification.maxCharacters")}");
            return;
        }
        _characterEditor.Begin(slot);
        if (_characterEditor.IsVisible) _uiView = "editor";
    }

    private void SelectCharacter(int slot)
    {
        if (_characters == null) return;
        var profile = _characters.Get(slot);
        if (profile == null) { BeginCharacterCreation(slot); return; }
        EnsureCharacterPortrait(slot);
        if (!GtaCharacterAppearance.Apply(profile))
        {
            Notification.Show($"~r~{T("ui.notification.characterLoadFailed")}");
            return;
        }
        _characters.SetActive(slot);
        _uiView = "hidden";
        Notification.Show($"~p~Project Liberty~w~: {T("ui.notification.characterSelected")} {profile.FullName}");
    }

    private void DeleteCharacter(int slot)
    {
        if (_characters == null) return;
        if (slot < 1 || slot > CharacterProfileRules.MaximumCharacters) return;
        var profile = _characters.Get(slot);
        if (profile == null || !_characters.Delete(slot)) return;

        var portrait = PortraitPath(slot);
        try
        {
            if (File.Exists(portrait)) File.Delete(portrait);
        }
        catch (Exception exception)
        {
            _logger?.LogWarning($"Character slot {slot} was deleted, but its portrait could not be removed: {exception.Message}");
        }
        _portraitCache.Remove(slot);

        if (_characters.ActiveSlot is int activeSlot)
        {
            var active = _characters.Get(activeSlot);
            if (active != null) GtaCharacterAppearance.Apply(active);
        }

        _logger?.LogInformation($"Deleted character {profile.FullName} from slot {slot}");
        Notification.Show($"~p~Project Liberty~w~: {T("ui.notification.characterDeleted")}");
        _lastSentUiView = string.Empty;
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!_initializationAttempted)
        {
            _initializationAttempted = true;
            Initialize();
        }
        if (!_initialized || _application == null)
            return;

        try
        {
            if (!_firstTickCompleted) _logger?.LogInformation("LibertyScript first game tick entered");
            _cefUi?.Tick();
            if (!_firstTickCompleted) _logger?.LogInformation("LibertyScript first idle CEF tick completed");
            var cefUiError = _cefUi?.LastError;
            if (!_uiErrorShown && !string.IsNullOrWhiteSpace(cefUiError))
            {
                _uiErrorShown = true;
                Notification.Show($"~r~Project Liberty CEF UI: {cefUiError}");
            }
            if (_pendingActiveCharacterApply && _characters?.ActiveSlot is int activeSlot)
            {
                _pendingActiveCharacterApply = false;
                var profile = _characters.Get(activeSlot);
                if (profile != null) GtaCharacterAppearance.Apply(profile);
            }
            _application.GetContainer().GetService<ProjectLiberty.Abstractions.GameHost.IGameLoop>().Tick();
            if (!_firstTickCompleted) _logger?.LogInformation("LibertyScript first core game-loop tick completed");
            _cefTest?.Tick();
            ProcessUiActions();
            UpdateReactUi();
            if (!_firstTickCompleted)
            {
                _firstTickCompleted = true;
                _logger?.LogInformation("LibertyScript first game tick completed");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "LibertyScript tick failed");
            Notification.Show($"~r~Menu error: {ex.Message}");
            Debug.WriteLine($"Menu error: {ex}");
        }
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (!_initialized || _application == null) return;

        var modifiers = ProjectLiberty.Abstractions.GameHost.ModifierKeys.None;
        if (e.Shift) modifiers |= ProjectLiberty.Abstractions.GameHost.ModifierKeys.Shift;
        if (e.Control) modifiers |= ProjectLiberty.Abstractions.GameHost.ModifierKeys.Control;
        if (e.Alt) modifiers |= ProjectLiberty.Abstractions.GameHost.ModifierKeys.Alt;
        _application.GetContainer().GetService<ProjectLiberty.Abstractions.GameHost.IGameLoop>().ProcessInput(
            new ProjectLiberty.Abstractions.GameHost.InputState { IsPressed = true, KeyCode = (int)e.KeyCode, Modifiers = modifiers });

        if (e.KeyCode == System.Windows.Forms.Keys.Escape && _uiView != "hidden")
        {
            SuppressPauseMenu(500);
            if (_characterEditor?.IsVisible == true) _characterEditor.Cancel();
            else _uiView = "hidden";
            return;
        }
        if (e.KeyCode == System.Windows.Forms.Keys.F10)
        {
            _cefTest?.Toggle();
            return;
        }
        if (e.KeyCode != System.Windows.Forms.Keys.F2) return;
        if (_uiView == "hidden")
        {
            _cefUi?.EnsureStarted();
            if (_cefUi?.IsStarted != true)
            {
                Notification.Show("~r~Project Liberty CEF UI недоступен. Выполните build.bat и установите результат целиком");
                return;
            }
            if (_characters?.ActiveSlot is int activeSlot) EnsureCharacterPortrait(activeSlot);
            _uiView = "dashboard";
        }
        else
        {
            if (_characterEditor?.IsVisible == true) _characterEditor.Cancel();
            _uiView = "hidden";
        }
    }

    private void ProcessUiActions()
    {
        while (_cefUi?.TryDequeueAction(out var cefMessage) == true) ProcessUiAction(cefMessage);
    }

    private void ProcessUiAction(Newtonsoft.Json.Linq.JObject message)
    {
        var action = message.Value<string>("action") ?? string.Empty;
        var slot = message.Value<int?>("slot") ?? 0;
        switch (action)
        {
            case "close": _uiView = "hidden"; break;
            case "future": Notification.Show($"~p~Project Liberty~w~: {T("ui.notification.future")}"); break;
            case "create-character": BeginCharacterCreation(slot); break;
            case "select-character": SelectCharacter(slot); break;
            case "delete-character": DeleteCharacter(slot); break;
            case "editor-section": _characterEditor?.SetSection(message.Value<int>("section")); break;
            case "editor-scroll": _characterEditor?.ScrollRows(message.Value<int>("delta")); break;
            case "editor-adjust": _characterEditor?.Adjust(message.Value<int>("index"), message.Value<int>("delta"), message.Value<bool?>("texture") ?? false); break;
            case "editor-set": _characterEditor?.SetAppearanceValue(message.Value<int>("index"), message.Value<float>("value")); break;
            case "editor-next": _characterEditor?.NextSection(); break;
            case "editor-save": _characterEditor?.Save(); break;
            case "editor-cancel": _characterEditor?.Cancel(); break;
            case "editor-rotate": _characterEditor?.Rotate(message.Value<float>("delta")); break;
            case "editor-zoom": _characterEditor?.Zoom(message.Value<int>("delta")); break;
            case "editor-edit-name":
                SendUiState("hidden");
                _characterEditor?.EditName(message.Value<int>("index"));
                _uiView = "editor";
                break;
        }
    }

    private void UpdateReactUi()
    {
        if (_uiView == "editor") _characterEditor?.UpdateWorldOnly();
        if (_uiView != "hidden")
        {
            SuppressPauseMenu();
            Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
            Game.DisableAllControlsThisFrame();
        }
        else if (Game.GameTime < _suppressPauseUntil)
        {
            Function.Call(Hash.SET_PAUSE_MENU_ACTIVE, false);
        }
        if (!string.Equals(_lastSentUiView, _uiView, StringComparison.Ordinal) ||
            Game.GameTime >= _nextUiStateAt)
        {
            _lastSentUiView = _uiView;
            _nextUiStateAt = Game.GameTime + 33;
            SendUiState(_uiView);
        }
    }

    private void SuppressPauseMenu(int keepSuppressedForMilliseconds = 0)
    {
        if (keepSuppressedForMilliseconds > 0)
            _suppressPauseUntil = Math.Max(_suppressPauseUntil, Game.GameTime + keepSuppressedForMilliseconds);

        Function.Call(Hash.SET_PAUSE_MENU_ACTIVE, false);
        Function.Call(Hash.DISABLE_FRONTEND_THIS_FRAME);
    }

    private void SendUiState(string view)
    {
        if (_characters == null) return;
        var characters = _characters.GetAll().Select(profile => new
        {
            slot = profile.Slot,
            fullName = profile.FullName,
            gender = T(profile.Gender == CharacterGender.Female ? "ui.editor.gender.female" : "ui.editor.gender.male"),
            active = _characters.ActiveSlot == profile.Slot,
            portrait = GetPortraitDataUri(profile.Slot)
        }).ToArray();
        object? editor = null;
        if (view == "editor" && _characterEditor != null)
        {
            editor = new
            {
                slot = _characterEditor.Slot,
                section = _characterEditor.Section,
                sectionTitle = _characterEditor.SectionTitle,
                fullName = _characterEditor.FullName,
                gender = _characterEditor.GenderName,
                rows = _characterEditor.GetVisibleRows().Select(row => new
                {
                    index = row.Index,
                    label = row.Label,
                    value = row.Value,
                    secondaryValue = row.SecondaryValue,
                    kind = row.Kind,
                    numericValue = row.NumericValue,
                    min = row.Minimum,
                    max = row.Maximum,
                    step = row.Step
                }).ToArray(),
                canScrollUp = _characterEditor.CanScrollUp,
                canScrollDown = _characterEditor.CanScrollDown
            };
        }
        var state = new
        {
            type = "state",
            view,
            language = _localization?.CurrentLanguage ?? "en",
            translations = _localizationCatalog?.GetStrings("ui.") ?? new Dictionary<string, string>(),
            characters,
            activeSlot = _characters.ActiveSlot,
            editor
        };
        _cefUi?.SendState(state, view);
    }

    private string T(string key) => _localization?.GetString(key) ?? key;

    private void CaptureCharacterPortrait(int slot)
    {
        try
        {
            _cefUi?.HideForCapture();
            Script.Wait(100);
            var path = PortraitPath(slot);
            if (CharacterPortraitCapture.Capture(path))
            {
                _portraitCache.Remove(slot);
                _logger?.LogInformation($"Captured portrait for character slot {slot}");
            }
            else _logger?.LogWarning($"Could not capture portrait for character slot {slot}");
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, $"Failed to capture portrait for character slot {slot}");
        }
    }

    private void EnsureCharacterPortrait(int slot)
    {
        if (File.Exists(PortraitPath(slot)) || _characterEditor == null || _characterEditor.IsVisible) return;
        _characterEditor.Begin(slot);
        if (!_characterEditor.IsVisible) return;
        _characterEditor.SetSection(3);
        CaptureCharacterPortrait(slot);
        _characterEditor.Cancel();
    }

    private string? GetPortraitDataUri(int slot)
    {
        var path = PortraitPath(slot);
        if (!File.Exists(path)) return null;
        var updated = File.GetLastWriteTimeUtc(path);
        if (_portraitCache.TryGetValue(slot, out var cached) && cached.UpdatedUtc == updated) return cached.DataUri;
        try
        {
            var uri = "data:image/jpeg;base64," + Convert.ToBase64String(File.ReadAllBytes(path));
            _portraitCache[slot] = (updated, uri);
            return uri;
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, $"Failed to load portrait for character slot {slot}");
            return null;
        }
    }

    private string PortraitPath(int slot) => Path.Combine(_baseDirectory, "Saves", "Portraits", $"character-{slot}.jpg");

    private void OnAborted(object sender, EventArgs e)
    {
        try
        {
            _uiView = "hidden";
            if (_characterEditor?.IsVisible == true) _characterEditor.Cancel();
            _cefUi?.Dispose();
            _cefTest?.Dispose();
            _application?.Shutdown();
        }
        finally
        {
            _container?.Dispose();
            _container = null;
            _logger = null;
            _localization = null;
            _localizationCatalog = null;
            _application = null;
            _characters = null;
            _characterEditor = null;
            _cefUi = null;
            _cefTest = null;
            _portraitCache.Clear();
            _initialized = false;
        }
    }
}
