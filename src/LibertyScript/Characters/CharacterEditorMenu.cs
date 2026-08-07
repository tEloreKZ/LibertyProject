using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using ProjectLiberty.Abstractions.UI;
using ProjectLiberty.Core.Characters;
using Font = GTA.UI.Font;

namespace ProjectLiberty.LibertyScript.Characters;

internal sealed class CharacterEditorMenu : IMenu
{
    private const float DesignWidth = 1280f;
    private const float DesignHeight = 720f;
    private static readonly Color Pink = Color.FromArgb(255, 235, 54, 119);
    // Michael's Rockford Hills mansion bedroom: a stable, detailed base-game interior.
    private static readonly Vector3 WardrobePosition = new Vector3(-813.60f, 179.47f, 71.15f);
    private static readonly Vector3 InteriorAnchor = new Vector3(-802.311f, 175.056f, 72.8446f);
    private const float WardrobeHeading = 111.12f;
    private static readonly string[] AppearanceNames =
    {
        "Пол", "Отец", "Мать", "Сходство", "Тон кожи", "Цвет глаз", "Прическа", "Цвет волос",
        "Ширина носа", "Высота носа", "Длина носа", "Переносица", "Кончик носа", "Изгиб носа",
        "Высота бровей", "Глубина бровей", "Высота скул", "Ширина скул", "Ширина щек", "Разрез глаз",
        "Толщина губ", "Ширина челюсти", "Форма челюсти", "Высота подбородка", "Длина подбородка",
        "Ширина подбородка", "Ямочка подбородка", "Толщина шеи",
        "Дефекты кожи", "Борода", "Брови", "Старение", "Макияж", "Румянец", "Цвет лица",
        "Повреждения солнцем", "Помада", "Веснушки", "Волосы на груди", "Дефекты тела", "Доп. дефекты тела"
    };
    private static readonly (string Name, int Id, bool Prop)[] ClothingItems =
    {
        ("Маска", 1, false), ("Торс", 3, false), ("Ноги", 4, false), ("Сумки", 5, false),
        ("Обувь", 6, false), ("Аксессуары", 7, false), ("Майка", 8, false), ("Броня", 9, false),
        ("Нашивки", 10, false), ("Верх", 11, false), ("Головной убор", 0, true),
        ("Очки", 1, true), ("Аксессуар уха", 2, true), ("Часы", 6, true), ("Браслет", 7, true)
    };

    private readonly string _assetsDirectory;
    private readonly CharacterRepository _repository;
    private readonly Action _returnToDashboard;
    private readonly List<IMenuItem> _items = new List<IMenuItem>();
    private CharacterProfile? _profile;
    private CustomSprite? _shell;
    private CustomSprite? _pixel;
    private CustomSprite? _cursor;
    private string? _loadedShellFile;
    private Camera? _camera;
    private Vector3 _returnPosition;
    private float _returnHeading;
    private Model _returnModel;
    private bool _returnInvincible;
    private float _layoutScale = 1f;
    private float _layoutOffsetX;
    private float _layoutOffsetY;
    private float _pointerX;
    private float _pointerY;
    private float _cursorX;
    private float _cursorY;
    private float _lastDragX;
    private bool _dragging;
    private int _section;
    private int _rowOffset;

    public string Id => "liberty_character_editor";
    public string Title => "Редактор персонажа";
    public IEnumerable<IMenuItem> Items => _items;
    public bool IsVisible { get; private set; }
    public int Section => _section;
    public int Slot => _profile?.Slot ?? 0;
    public string FullName => _profile?.FullName ?? string.Empty;
    public string GenderName => _profile?.Gender == CharacterGender.Female ? "Женский персонаж" : "Мужской персонаж";
    public string SectionTitle => new[] { "ВНЕШНОСТЬ", "ОДЕЖДА", "ИМЯ И ДАННЫЕ", "ПРОСМОТР" }[_section];
    public bool CanScrollUp => _rowOffset > 0;
    public bool CanScrollDown => _rowOffset + 8 < CurrentRowCount;

    private int CurrentRowCount => _section == 0 ? AppearanceNames.Length : _section == 1 ? ClothingItems.Length : _section == 2 ? 2 : 0;

    public CharacterEditorMenu(string assetsDirectory, CharacterRepository repository, Action returnToDashboard)
    {
        _assetsDirectory = assetsDirectory;
        _repository = repository;
        _returnToDashboard = returnToDashboard;
    }

    public void Begin(int slot)
    {
        CharacterProfileRules.ValidateSlot(slot);
        if (IsVisible) return;
        var ped = Game.Player.Character;
        _returnPosition = ped.Position;
        _returnHeading = ped.Heading;
        _returnModel = ped.Model;
        _returnInvincible = ped.IsInvincible;
        _profile = _repository.Get(slot) ?? CharacterProfile.CreateDefault(slot);
        _section = 0;
        _rowOffset = 0;

        LoadMansionInterior();
        if (!GtaCharacterAppearance.Apply(_profile))
        {
            Notification.Show("~r~Не удалось загрузить freemode-модель GTA Online");
            return;
        }
        PlacePed();
        PreparePed();
        CreateEditorCamera();
        World.RenderingCamera = _camera;
        IsVisible = true;
    }

    public void Show()
    {
        var slot = _repository.FirstFreeSlot();
        if (slot > 0) Begin(slot);
    }

    public void Hide() => CloseEditor(false);
    public void AddItem(IMenuItem item) { if (item != null) _items.Add(item); }
    public void RemoveItem(string itemId) => _items.RemoveAll(x => x.Id == itemId);

    public void Update()
    {
        if (!IsVisible || _profile == null) return;
        Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
        Game.DisableAllControlsThisFrame();
        Game.EnableControlThisFrame(Control.FrontendCancel);
        Function.Call(Hash.SET_MOUSE_CURSOR_THIS_FRAME);
        PlacePed();
        UpdateLayout();
        ReadPointer();

        if (Game.IsEnabledControlJustPressed(Control.FrontendCancel))
        {
            CloseEditor(false);
            return;
        }

        EnsureAssets();
        if (_shell != null)
        {
            Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 0);
            _shell.Position = new PointF(_layoutOffsetX, _layoutOffsetY);
            _shell.Size = new SizeF(DesignWidth * _layoutScale, DesignHeight * _layoutScale);
            _shell.ScaledDraw();
        }
        Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 7);
        DrawDynamicContent();
        HandleMouse();
        DrawCursor();
    }

    public void UpdateWorldOnly()
    {
        if (!IsVisible) return;
        Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
        PlacePed();
    }

    public void SetSection(int section)
    {
        if (section < 0 || section > 3) return;
        _section = section;
        _rowOffset = 0;
        UpdateCameraForSection();
    }

    public void ScrollRows(int delta)
    {
        _rowOffset = Math.Max(0, Math.Min(Math.Max(0, CurrentRowCount - 8), _rowOffset + Math.Sign(delta)));
    }

    public void Adjust(int index, int delta, bool texture)
    {
        if (_section == 0 && index >= 0 && index < AppearanceNames.Length) ChangeAppearance(index, Math.Sign(delta));
        else if (_section == 1 && index >= 0 && index < ClothingItems.Length) ChangeClothing(index, Math.Sign(delta), texture);
    }

    public void NextSection()
    {
        if (_section < 3)
        {
            _section++;
            _rowOffset = 0;
            UpdateCameraForSection();
        }
    }

    public void Save() => SaveAndClose();
    public void Cancel() => CloseEditor(false);
    public void EditName(int index) { if (index is 0 or 1) EditIdentity(index); }

    public void Rotate(float delta)
    {
        if (!IsVisible) return;
        Game.Player.Character.Heading += Math.Max(-40f, Math.Min(40f, delta)) * .45f;
    }

    public void Zoom(int delta)
    {
        if (_camera == null) return;
        _camera.FieldOfView = Clamp(_camera.FieldOfView + Math.Sign(delta) * 2f, 18f, 48f);
    }

    public IReadOnlyList<CharacterEditorRow> GetVisibleRows()
    {
        var rows = new List<CharacterEditorRow>();
        if (_profile == null) return rows;
        for (var visible = 0; visible < 8; visible++)
        {
            var index = _rowOffset + visible;
            if (index >= CurrentRowCount) break;
            if (_section == 0)
                rows.Add(new CharacterEditorRow(index, AppearanceNames[index], AppearanceValue(index)));
            else if (_section == 1)
            {
                var item = ClothingItems[index];
                if (item.Prop)
                {
                    var prop = _profile.Props[item.Id];
                    rows.Add(new CharacterEditorRow(index, item.Name, prop.Drawable < 0 ? "Нет" : prop.Drawable.ToString(), prop.Texture.ToString()));
                }
                else
                {
                    var component = _profile.Components[item.Id];
                    rows.Add(new CharacterEditorRow(index, item.Name, component.Drawable.ToString(), component.Texture.ToString()));
                }
            }
            else if (_section == 2)
                rows.Add(new CharacterEditorRow(index, index == 0 ? "Имя" : "Фамилия", index == 0 ? _profile.FirstName : _profile.LastName, kind: "text"));
        }
        return rows;
    }

    private void HandleMouse()
    {
        var clicked = Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)Control.CursorAccept);
        var pressed = Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, (int)Control.CursorAccept);
        for (var section = 0; section < 4; section++)
        {
            var region = new RectangleF(18, 128 + section * 48, 234, 40);
            if (!Contains(region)) continue;
            DrawHover(region);
            if (clicked) { _section = section; _rowOffset = 0; }
        }

        var panelRows = _section == 0 ? AppearanceNames.Length : _section == 1 ? ClothingItems.Length : _section == 2 ? 2 : 0;
        if (Contains(new RectangleF(895, 145, 362, 410)))
        {
            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)Control.CursorScrollDown))
                _rowOffset = Math.Min(Math.Max(0, panelRows - 8), _rowOffset + 1);
            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)Control.CursorScrollUp))
                _rowOffset = Math.Max(0, _rowOffset - 1);
        }

        for (var visibleRow = 0; visibleRow < 8; visibleRow++)
        {
            var itemIndex = _rowOffset + visibleRow;
            if (itemIndex >= panelRows) break;
            var row = new RectangleF(919, 156 + visibleRow * 39, 314, 31);
            if (!Contains(row)) continue;
            DrawHover(row);
            if (!clicked) continue;
            if (_section == 2) EditIdentity(itemIndex);
            else
            {
                var delta = _cursorX < 1124 ? -1 : 1;
                var texture = _cursorX >= 1124 && _cursorX <= 1198;
                if (_section == 0) ChangeAppearance(itemIndex, delta);
                else ChangeClothing(itemIndex, delta, texture);
            }
        }

        var next = new RectangleF(919, 576, 314, 47);
        if (Contains(next))
        {
            DrawHover(next, 42);
            if (clicked)
            {
                if (_section < 3) { _section++; _rowOffset = 0; }
                else SaveAndClose();
            }
        }

        var modelArea = new RectangleF(270, 100, 625, 540);
        if (Contains(modelArea) && pressed)
        {
            if (!_dragging) { _dragging = true; _lastDragX = _cursorX; }
            var ped = Game.Player.Character;
            ped.Heading += (_cursorX - _lastDragX) * 0.45f;
            _lastDragX = _cursorX;
        }
        else _dragging = false;

        if (Contains(modelArea) && _camera != null)
        {
            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)Control.CursorScrollUp))
                _camera.FieldOfView = Math.Max(24f, _camera.FieldOfView - 2f);
            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)Control.CursorScrollDown))
                _camera.FieldOfView = Math.Min(55f, _camera.FieldOfView + 2f);
        }
    }

    private void ChangeAppearance(int index, int delta)
    {
        if (_profile == null) return;
        if (index == 0)
        {
            _profile.Gender = _profile.Gender == CharacterGender.Male ? CharacterGender.Female : CharacterGender.Male;
            var defaults = CharacterProfile.CreateDefault(_profile.Slot);
            _profile.Components = defaults.Components;
            _profile.Props = defaults.Props;
            _profile.HairDrawable = 0;
            _profile.HairTexture = 0;
        }
        else if (index == 1) _profile.FatherId = Wrap(_profile.FatherId + delta, 46);
        else if (index == 2) _profile.MotherId = Wrap(_profile.MotherId + delta, 46);
        else if (index == 3) _profile.ShapeMix = Clamp(_profile.ShapeMix + delta * .05f, 0f, 1f);
        else if (index == 4) _profile.SkinMix = Clamp(_profile.SkinMix + delta * .05f, 0f, 1f);
        else if (index == 5) _profile.EyeColor = Wrap(_profile.EyeColor + delta, 32);
        else if (index == 6)
            _profile.HairDrawable = Wrap(_profile.HairDrawable + delta, GtaCharacterAppearance.DrawableCount(Game.Player.Character, 2));
        else if (index == 7) _profile.HairColor = Wrap(_profile.HairColor + delta, 64);
        else if (index < 28) _profile.FaceFeatures[index - 8] = Clamp(_profile.FaceFeatures[index - 8] + delta * .1f, -1f, 1f);
        else
        {
            var overlay = _profile.Overlays[index - 28];
            var count = Math.Max(1, Function.Call<int>(Hash.GET_PED_HEAD_OVERLAY_NUM, overlay.OverlayId));
            overlay.Value = Wrap(overlay.Value + 1 + delta, count + 1) - 1;
        }
        GtaCharacterAppearance.Apply(_profile);
        PlacePed();
        PreparePed();
        _camera?.PointAt(Game.Player.Character.Position + new Vector3(0f, 0f, .65f));
    }

    private void ChangeClothing(int index, int delta, bool texture)
    {
        if (_profile == null) return;
        var item = ClothingItems[index];
        var ped = Game.Player.Character;
        if (!item.Prop)
        {
            var component = _profile.Components[item.Id];
            component.IsSet = true;
            if (texture)
                component.Texture = Wrap(component.Texture + delta, GtaCharacterAppearance.TextureCount(ped, item.Id, component.Drawable));
            else
            {
                component.Drawable = Wrap(component.Drawable + delta, GtaCharacterAppearance.DrawableCount(ped, item.Id));
                component.Texture = 0;
            }
        }
        else
        {
            var prop = _profile.Props[item.Id];
            prop.IsSet = true;
            var count = GtaCharacterAppearance.PropDrawableCount(ped, item.Id);
            if (texture && prop.Drawable >= 0)
            {
                var textures = Math.Max(1, Function.Call<int>(Hash.GET_NUMBER_OF_PED_PROP_TEXTURE_VARIATIONS, ped.Handle, item.Id, prop.Drawable));
                prop.Texture = Wrap(prop.Texture + delta, textures);
            }
            else prop.Drawable = Wrap(prop.Drawable + 1 + delta, count + 1) - 1;
        }
        GtaCharacterAppearance.Apply(_profile);
        PlacePed();
        PreparePed();
    }

    private void EditIdentity(int index)
    {
        if (_profile == null) return;
        var current = index == 0 ? _profile.FirstName : _profile.LastName;
        var value = Game.GetUserInput(WindowTitle.EnterMessage20, current, 20)?.Trim();
        if (string.IsNullOrWhiteSpace(value)) return;
        if (index == 0) _profile.FirstName = value!;
        else _profile.LastName = value!;
    }

    private void SaveAndClose()
    {
        if (_profile == null) return;
        CharacterProfileRules.Normalize(_profile);
        _repository.Save(_profile);
        CloseEditor(true);
        Notification.Show($"~p~Project Liberty~w~: {_profile.FullName} сохранен");
    }

    private void CloseEditor(bool keepCreatedModel)
    {
        if (!IsVisible && _camera == null) return;
        IsVisible = false;
        World.RenderingCamera = null;
        _camera?.Delete();
        _camera = null;
        if (!keepCreatedModel && _returnModel.IsValid) Game.Player.ChangeModel(_returnModel);
        var ped = Game.Player.Character;
        ped.Position = _returnPosition;
        ped.Heading = _returnHeading;
        ped.IsPositionFrozen = false;
        ped.IsInvincible = _returnInvincible;
        Function.Call(Hash.CLEAR_FOCUS);
        _returnToDashboard();
    }

    private static void LoadMansionInterior()
    {
        Function.Call(Hash.SET_FOCUS_POS_AND_VEL, InteriorAnchor.X, InteriorAnchor.Y, InteriorAnchor.Z, 0f, 0f, 0f);
        var interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, InteriorAnchor.X, InteriorAnchor.Y, InteriorAnchor.Z);
        if (interior != 0)
        {
            Function.Call(Hash.DISABLE_INTERIOR, interior, false);
            Function.Call(Hash.SET_INTERIOR_ACTIVE, interior, true);
            Function.Call(Hash.PIN_INTERIOR_IN_MEMORY, interior);
            Function.Call(Hash.REFRESH_INTERIOR, interior);
            for (var attempt = 0; attempt < 40 && !Function.Call<bool>(Hash.IS_INTERIOR_READY, interior); attempt++)
                Script.Wait(50);
        }
    }

    private void CreateEditorCamera()
    {
        var ped = Game.Player.Character;
        _camera = World.CreateCamera(ped.GetOffsetPosition(new Vector3(0f, 1.45f, .72f)), Vector3.Zero, 30f);
        UpdateCameraForSection();
        World.RenderingCamera = _camera;
    }

    private void UpdateCameraForSection()
    {
        if (_camera == null) return;
        var ped = Game.Player.Character;
        var distance = _section switch
        {
            0 => 1.45f,
            1 => 2.55f,
            2 => 1.75f,
            _ => 2.25f
        };
        var height = _section == 1 || _section == 3 ? .70f : .76f;
        _camera.Position = ped.GetOffsetPosition(new Vector3(0f, distance, height));
        _camera.FieldOfView = _section == 0 ? 30f : _section == 2 ? 33f : 38f;
        _camera.PointAt(ped.Position + new Vector3(0f, 0f, height));
    }

    private static void PlacePed()
    {
        var ped = Game.Player.Character;
        ped.Position = WardrobePosition;
        if (!ped.IsPositionFrozen) ped.Heading = WardrobeHeading;
        ped.IsPositionFrozen = true;
        ped.IsInvincible = true;
        ped.IsVisible = true;
        ped.Opacity = 255;
    }

    private static void PreparePed()
    {
        var ped = Game.Player.Character;
        Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped.Handle);
        Function.Call(Hash.TASK_STAND_STILL, ped.Handle, -1);
    }

    private void DrawDynamicContent()
    {
        if (_profile == null) return;
        var sectionNames = new[] { "Внешность", "Одежда", "Имя и данные", "Предпросмотр" };
        var stepNames = new[] { "ВНЕШНОСТЬ", "ОДЕЖДА", "ИМЯ И ДАННЫЕ", "ПРОСМОТР" };
        for (var index = 0; index < sectionNames.Length; index++)
        {
            var region = new RectangleF(18, 128 + index * 48, 234, 40);
            DrawBlock(region, index == _section ? Color.FromArgb(225, 69, 18, 40) : Color.FromArgb(225, 5, 10, 14));
            DrawText("●", 43, 147 + index * 48, .25f, index == _section ? Pink : Color.FromArgb(255, 154, 160, 167), false, Alignment.Center);
            DrawText(sectionNames[index], 67, 140 + index * 48, .31f, index == _section ? Color.White : Color.FromArgb(255, 154, 160, 167));
            DrawText(stepNames[index], 460 + index * 115, 65, .22f,
                index == _section ? Pink : Color.FromArgb(255, 154, 160, 167), true, Alignment.Center);
        }
        DrawText($"Персонаж {_profile.Slot} / 3", 1085, 50, .28f, Pink, true);
        DrawText(new[] { "ВНЕШНОСТЬ", "ОДЕЖДА", "ИМЯ И ДАННЫЕ", "ПРОСМОТР" }[_section], 919, 124, .34f, Color.White, true);

        if (_section == 0)
        {
            for (var row = 0; row < 8 && _rowOffset + row < AppearanceNames.Length; row++)
            {
                var index = _rowOffset + row;
                DrawRow(row, AppearanceNames[index], AppearanceValue(index));
            }
        }
        else if (_section == 1)
        {
            for (var row = 0; row < 8 && _rowOffset + row < ClothingItems.Length; row++)
            {
                var index = _rowOffset + row;
                DrawRow(row, ClothingItems[index].Name, ClothingValue(index));
            }
        }
        else if (_section == 2)
        {
            DrawRow(0, "Имя", _profile.FirstName);
            DrawRow(1, "Фамилия", _profile.LastName);
            DrawText("Нажмите на строку, чтобы ввести текст", 919, 260, .25f, Color.FromArgb(255, 154, 160, 167));
        }
        else
        {
            DrawText(_profile.FullName, 919, 180, .45f, Color.White, true);
            DrawText(_profile.Gender == CharacterGender.Male ? "Мужской персонаж" : "Женский персонаж", 919, 220, .29f, Pink);
            DrawText("Внешность и одежда будут сохранены в этом слоте.", 919, 270, .25f, Color.FromArgb(255, 154, 160, 167));
            DrawText("Нажмите СОХРАНИТЬ для завершения.", 919, 300, .25f, Color.FromArgb(255, 154, 160, 167));
        }
        DrawText(_section == 3 ? "СОХРАНИТЬ" : "ДАЛЕЕ", 1076, 592, .36f, Color.White, true, Alignment.Center);
    }

    private void DrawRow(int row, string name, string value)
    {
        var y = 165 + row * 39;
        DrawText(name, 919, y, .27f, Color.FromArgb(255, 205, 208, 212));
        DrawText(value, 1161, y, .27f, Color.White, true, Alignment.Center);
    }

    private string AppearanceValue(int index)
    {
        if (_profile == null) return string.Empty;
        if (index == 0) return _profile.Gender == CharacterGender.Male ? "Мужской" : "Женский";
        if (index == 1) return _profile.FatherId.ToString();
        if (index == 2) return _profile.MotherId.ToString();
        if (index == 3) return $"{_profile.ShapeMix:P0}";
        if (index == 4) return $"{_profile.SkinMix:P0}";
        if (index == 5) return _profile.EyeColor.ToString();
        if (index == 6) return _profile.HairDrawable.ToString();
        if (index == 7) return _profile.HairColor.ToString();
        if (index < 28) return $"{_profile.FaceFeatures[index - 8]:0.0}";
        return _profile.Overlays[index - 28].Value < 0 ? "Нет" : _profile.Overlays[index - 28].Value.ToString();
    }

    private string ClothingValue(int index)
    {
        if (_profile == null) return string.Empty;
        var item = ClothingItems[index];
        if (item.Prop)
        {
            var prop = _profile.Props[item.Id];
            return prop.Drawable < 0 ? "Нет" : $"{prop.Drawable} / {prop.Texture}";
        }
        var component = _profile.Components[item.Id];
        return $"{component.Drawable} / {component.Texture}";
    }

    private void EnsureAssets()
    {
        var file = SelectAsset(Screen.Resolution.Height);
        if (!string.Equals(file, _loadedShellFile, StringComparison.OrdinalIgnoreCase))
        {
            _shell = LoadSprite(file, new SizeF(DesignWidth, DesignHeight));
            _loadedShellFile = file;
        }
        _pixel ??= LoadSprite("white-pixel.png", new SizeF(1, 1), Color.White);
        _cursor ??= LoadSprite("cursor.png", new SizeF(20, 28));
    }

    private static string SelectAsset(int height)
    {
        if (height <= 720) return "character-editor-1280x720.png";
        if (height <= 1080) return "character-editor-1920x1080.png";
        if (height <= 1440) return "character-editor-2560x1440.png";
        return "character-editor-3840x2160.png";
    }

    private void ReadPointer()
    {
        _pointerX = Clamp(Game.GetDisabledControlValueNormalized(Control.CursorX), 0f, 1f) * Screen.ScaledWidth;
        _pointerY = Clamp(Game.GetDisabledControlValueNormalized(Control.CursorY), 0f, 1f) * DesignHeight;
        _cursorX = (_pointerX - _layoutOffsetX) / _layoutScale;
        _cursorY = (_pointerY - _layoutOffsetY) / _layoutScale;
    }

    private void DrawHover(RectangleF region, int alpha = 28)
    {
        if (_pixel == null) return;
        _pixel.Position = new PointF(_layoutOffsetX + region.X * _layoutScale, _layoutOffsetY + region.Y * _layoutScale);
        _pixel.Size = new SizeF(region.Width * _layoutScale, region.Height * _layoutScale);
        _pixel.Color = Color.FromArgb(alpha, Pink.R, Pink.G, Pink.B);
        _pixel.ScaledDraw();
    }

    private void DrawBlock(RectangleF region, Color color)
    {
        if (_pixel == null) return;
        _pixel.Position = new PointF(_layoutOffsetX + region.X * _layoutScale, _layoutOffsetY + region.Y * _layoutScale);
        _pixel.Size = new SizeF(region.Width * _layoutScale, region.Height * _layoutScale);
        _pixel.Color = color;
        _pixel.ScaledDraw();
    }

    private void DrawCursor()
    {
        if (_cursor == null) return;
        _cursor.Position = new PointF(_pointerX, _pointerY);
        _cursor.Size = new SizeF(20 * _layoutScale, 28 * _layoutScale);
        _cursor.ScaledDraw();
    }

    private void DrawText(string value, float x, float y, float scale, Color color, bool bold = false, Alignment alignment = Alignment.Left)
    {
        new TextElement(value, new PointF(_layoutOffsetX + x * _layoutScale, _layoutOffsetY + y * _layoutScale),
            scale * _layoutScale, color, bold ? Font.ChaletComprimeCologne : Font.ChaletLondon, alignment).ScaledDraw();
    }

    private bool Contains(RectangleF region) => region.Contains(_cursorX, _cursorY);

    private void UpdateLayout()
    {
        _layoutScale = Math.Min(Screen.ScaledWidth / DesignWidth, 1f);
        _layoutOffsetX = (Screen.ScaledWidth - DesignWidth * _layoutScale) / 2f;
        _layoutOffsetY = (DesignHeight - DesignHeight * _layoutScale) / 2f;
    }

    private CustomSprite? LoadSprite(string file, SizeF size, Color? color = null)
    {
        var path = Path.Combine(_assetsDirectory, file);
        if (!File.Exists(path)) return null;
        return color.HasValue ? new CustomSprite(path, size, PointF.Empty, color.Value) : new CustomSprite(path, size, PointF.Empty);
    }

    private static int Wrap(int value, int count) => count <= 0 ? 0 : ((value % count) + count) % count;
    private static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
}

internal sealed class CharacterEditorRow
{
    public int Index { get; }
    public string Label { get; }
    public string Value { get; }
    public string? SecondaryValue { get; }
    public string Kind { get; }

    public CharacterEditorRow(int index, string label, string value, string? secondaryValue = null, string kind = "value")
    {
        Index = index;
        Label = label;
        Value = value;
        SecondaryValue = secondaryValue;
        Kind = kind;
    }
}
