using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.UI;
using ProjectLiberty.Abstractions.UI;
using UiFont = GTA.UI.Font;

namespace ProjectLiberty.UI;

/// <summary>
/// Full-screen Project Liberty dashboard rendered as a deterministic visual shell
/// with mouse-driven interactive regions layered above it.
/// </summary>
public sealed class DashboardMenu : IMenu
{
    private const float DesignWidth = 1280f;
    private const float DesignHeight = 720f;
    private static readonly Color Pink = Color.FromArgb(255, 235, 54, 119);

    private readonly string _assetsDirectory;
    private readonly Func<IReadOnlyList<CharacterMenuEntry>> _getCharacters;
    private readonly Action<int>? _createCharacter;
    private readonly Action<int>? _selectCharacter;
    private readonly List<IMenuItem> _items = new List<IMenuItem>();
    private CustomSprite? _shell;
    private CustomSprite? _hover;
    private CustomSprite? _cursor;
    private bool _assetsLoaded;
    private string? _loadedShellFile;
    private float _cursorX;
    private float _cursorY;
    private float _pointerX;
    private float _pointerY;
    private float _layoutScale = 1f;
    private float _layoutOffsetX;
    private float _layoutOffsetY;
    private bool _clicked;

    private static readonly RectangleF[] ActionRegions =
    {
        new RectangleF(451, 330, 194, 32),
        new RectangleF(692, 144, 263, 47),
        new RectangleF(692, 199, 263, 47),
        new RectangleF(692, 256, 263, 47),
        new RectangleF(692, 321, 263, 47),
        new RectangleF(1004, 337, 217, 31),
        new RectangleF(1150, 410, 78, 24),
        new RectangleF(293, 609, 184, 67),
        new RectangleF(488, 609, 177, 67),
        new RectangleF(677, 609, 202, 67)
    };

    public string Id { get; }
    public string Title => "Project Liberty";
    public IEnumerable<IMenuItem> Items => _items;
    public bool IsVisible { get; private set; }

    public DashboardMenu(string id, string assetsDirectory)
        : this(id, assetsDirectory, () => Array.Empty<CharacterMenuEntry>(), null, null)
    {
    }

    public DashboardMenu(
        string id,
        string assetsDirectory,
        Func<IReadOnlyList<CharacterMenuEntry>> getCharacters,
        Action<int>? createCharacter,
        Action<int>? selectCharacter)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Menu ID cannot be empty.", nameof(id));
        Id = id;
        _assetsDirectory = assetsDirectory ?? throw new ArgumentNullException(nameof(assetsDirectory));
        _getCharacters = getCharacters ?? throw new ArgumentNullException(nameof(getCharacters));
        _createCharacter = createCharacter;
        _selectCharacter = selectCharacter;
    }

    public void AddItem(IMenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public void RemoveItem(string itemId) => _items.RemoveAll(item => item.Id == itemId);
    public void Show() => IsVisible = true;
    public void Hide() => IsVisible = false;

    public void Update()
    {
        if (!IsVisible) return;

        Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
        Game.DisableAllControlsThisFrame();
        Game.EnableControlThisFrame(Control.FrontendCancel);
        Function.Call(Hash.SET_MOUSE_CURSOR_THIS_FRAME);

        UpdateLayout();
        _pointerX = Clamp(Game.GetDisabledControlValueNormalized(Control.CursorX), 0f, 1f) * Screen.ScaledWidth;
        _pointerY = Clamp(Game.GetDisabledControlValueNormalized(Control.CursorY), 0f, 1f) * DesignHeight;
        _cursorX = (_pointerX - _layoutOffsetX) / _layoutScale;
        _cursorY = (_pointerY - _layoutOffsetY) / _layoutScale;
        _clicked = Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, (int)Control.CursorAccept);

        if (Game.IsEnabledControlJustPressed(Control.FrontendCancel))
        {
            Hide();
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
        if (_getCharacters().Count > 0) DrawCharacterSlots();
        DrawMouseInteractions();
        DrawCursor();
    }

    private void DrawMouseInteractions()
    {
        var characters = _getCharacters();
        if (characters.Count == 0)
        {
            var createRegion = new RectangleF(418, 486, 692, 106);
            if (Contains(createRegion))
            {
                DrawHover(createRegion, 32);
                if (_clicked) _createCharacter?.Invoke(1);
            }
            return;
        }

        for (var index = 0; index < 9; index++)
        {
            var region = new RectangleF(19, 122 + index * 40, 223, 38);
            if (!Contains(region)) continue;
            DrawHover(region, 48);
            if (_clicked && index != 0) ShowFuturePhaseNotice();
            return;
        }

        for (var index = 0; index < ActionRegions.Length; index++)
        {
            var region = ActionRegions[index];
            if (!Contains(region)) continue;
            DrawHover(region, 35);
            if (_clicked && index >= ActionRegions.Length - 3)
            {
                var slot = index - (ActionRegions.Length - 3) + 1;
                var entry = characters.FirstOrDefault(x => x.Slot == slot);
                if (entry != null) _selectCharacter?.Invoke(slot);
                else _createCharacter?.Invoke(slot);
            }
            else if (_clicked) ShowFuturePhaseNotice();
            return;
        }
    }

    private void DrawCharacterSlots()
    {
        var characters = _getCharacters();
        DrawBlock(new RectangleF(277, 579, 614, 106), Color.FromArgb(252, 9, 14, 18));
        DrawText("ПЕРСОНАЖИ", 294, 591, 0.34f, Color.White, true);
        for (var slot = 1; slot <= 3; slot++)
        {
            var x = slot == 1 ? 293f : slot == 2 ? 488f : 677f;
            var width = slot == 3 ? 202f : slot == 1 ? 184f : 177f;
            var region = new RectangleF(x, 609, width, 67);
            DrawBlock(region, Color.FromArgb(245, 20, 25, 29));
            var entry = characters.FirstOrDefault(item => item.Slot == slot);
            if (entry == null)
            {
                DrawText("+", x + 21, 626, 0.48f, Pink, true);
                DrawText($"Создать персонажа {slot}", x + 49, 619, 0.27f, Color.White, true);
                DrawText("Свободный слот", x + 49, 641, 0.22f, Color.FromArgb(255, 154, 159, 165));
            }
            else
            {
                DrawText(entry.FullName, x + 14, 619, 0.28f, Color.White, true);
                DrawText($"Персонаж {slot} / 3", x + 14, 641, 0.22f, Color.FromArgb(255, 154, 159, 165));
                DrawText(entry.IsActive ? "АКТИВЕН" : "ВЫБРАТЬ", x + 14, 658, 0.20f,
                    entry.IsActive ? Color.FromArgb(255, 125, 220, 133) : Pink, true);
            }
        }
    }

    private void DrawBlock(RectangleF region, Color color)
    {
        if (_hover == null) return;
        _hover.Position = new PointF(_layoutOffsetX + region.X * _layoutScale, _layoutOffsetY + region.Y * _layoutScale);
        _hover.Size = new SizeF(region.Width * _layoutScale, region.Height * _layoutScale);
        _hover.Color = color;
        _hover.ScaledDraw();
    }

    private void DrawText(string value, float x, float y, float scale, Color color, bool bold = false)
    {
        var element = new TextElement(value,
            new PointF(_layoutOffsetX + x * _layoutScale, _layoutOffsetY + y * _layoutScale),
            scale * _layoutScale, color, bold ? UiFont.ChaletComprimeCologne : UiFont.ChaletLondon);
        element.ScaledDraw();
    }

    private void DrawHover(RectangleF region, int alpha)
    {
        if (_hover == null) return;
        _hover.Position = new PointF(
            _layoutOffsetX + region.X * _layoutScale,
            _layoutOffsetY + region.Y * _layoutScale);
        _hover.Size = new SizeF(region.Width * _layoutScale, region.Height * _layoutScale);
        _hover.Color = Color.FromArgb(alpha, Pink.R, Pink.G, Pink.B);
        _hover.ScaledDraw();
    }

    private void DrawCursor()
    {
        if (_cursor == null) return;
        _cursor.Position = new PointF(_pointerX, _pointerY);
        _cursor.Size = new SizeF(20 * _layoutScale, 28 * _layoutScale);
        _cursor.ScaledDraw();
    }

    private bool Contains(RectangleF region)
    {
        return region.Contains(_cursorX, _cursorY);
    }

    private static void ShowFuturePhaseNotice()
    {
        Notification.Show("~p~Project Liberty~w~: функция будет доступна в следующих фазах разработки");
    }

    private void EnsureAssets()
    {
        var shellFile = SelectShellFile(Screen.Resolution.Height, _getCharacters().Count == 0);
        if (!string.Equals(_loadedShellFile, shellFile, StringComparison.OrdinalIgnoreCase))
        {
            _shell = LoadSprite(shellFile, new SizeF(DesignWidth, DesignHeight))
                ?? LoadSprite("dashboard-shell.png", new SizeF(DesignWidth, DesignHeight));
            _loadedShellFile = shellFile;
        }
        if (_assetsLoaded) return;
        _assetsLoaded = true;
        _hover = LoadSprite("white-pixel.png", new SizeF(1, 1), Color.White);
        _cursor = LoadSprite("cursor.png", new SizeF(20, 28));
    }

    private static string SelectShellFile(int screenHeight, bool empty)
    {
        var prefix = empty ? "empty-dashboard" : "dashboard-shell";
        if (screenHeight <= 720) return $"{prefix}-1280x720.png";
        if (screenHeight <= 1080) return $"{prefix}-1920x1080.png";
        if (screenHeight <= 1440) return $"{prefix}-2560x1440.png";
        return $"{prefix}-3840x2160.png";
    }

    private void UpdateLayout()
    {
        _layoutScale = Math.Min(Screen.ScaledWidth / DesignWidth, 1f);
        _layoutOffsetX = (Screen.ScaledWidth - DesignWidth * _layoutScale) / 2f;
        _layoutOffsetY = (DesignHeight - DesignHeight * _layoutScale) / 2f;
    }

    private CustomSprite? LoadSprite(string fileName, SizeF size, Color? color = null)
    {
        var path = Path.Combine(_assetsDirectory, fileName);
        if (!File.Exists(path)) return null;
        return color.HasValue
            ? new CustomSprite(path, size, PointF.Empty, color.Value)
            : new CustomSprite(path, size, PointF.Empty);
    }

    private static float Clamp(float value, float minimum, float maximum)
    {
        return value < minimum ? minimum : value > maximum ? maximum : value;
    }
}

public sealed class CharacterMenuEntry
{
    public int Slot { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
