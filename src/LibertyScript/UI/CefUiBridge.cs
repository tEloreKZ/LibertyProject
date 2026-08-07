using System.Collections.Concurrent;
using GTA.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectLiberty.UI;

namespace ProjectLiberty.LibertyScript.UI;

internal sealed class CefUiBridge : IDisposable
{
    private const string OwnerId = "project-liberty.shvdn.main-ui";
    private readonly ConcurrentQueue<JObject> _actions = new ConcurrentQueue<JObject>();
    private Browser? _browser;
    private string? _lastStateJson;
    private JObject? _pendingState;
    private string _view = "hidden";
    private int _width;
    private int _height;
    private bool _pageReady;
    private bool _disposed;

    public bool IsStarted => _browser != null;
    public bool IsReady => _browser != null && _pageReady;
    public string? LastError { get; private set; }

    public void Tick()
    {
        // Do not cross the managed/native boundary while no browser exists. SHVDN may
        // instantiate scripts before GTA has delivered its first safe game tick.
        if (_disposed || _browser == null) return;
        try
        {
            Browser.DispatchPendingEvents();
            var resolution = Screen.Resolution;
            if (resolution.Width != _width || resolution.Height != _height)
            {
                _width = resolution.Width;
                _height = resolution.Height;
                _browser.SetBounds(0, 0, _width, _height);
            }
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
        }
    }

    public bool EnsureStarted()
    {
        if (_disposed) return false;
        if (_browser != null) return true;
        if (!Browser.IsNativeCoreReady) return false;
        try
        {
            CreateBrowser();
            return _browser != null;
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            return false;
        }
    }

    public void SendState(object state, string view)
    {
        if (_disposed) return;
        var json = JsonConvert.SerializeObject(state);
        _pendingState = JObject.Parse(json);
        SetView(view);
        if (!_pageReady || _browser == null || string.Equals(json, _lastStateJson, StringComparison.Ordinal)) return;
        try
        {
            _browser.Emit("ui:state", _pendingState);
            _lastStateJson = json;
        }
        catch (Exception exception) { LastError = exception.Message; }
    }

    public bool TryDequeueAction(out JObject action) => _actions.TryDequeue(out action!);

    public void HideForCapture()
    {
        if (_browser == null || !_pageReady) return;
        _browser.Unfocus();
        _browser.Hide();
    }

    private void CreateBrowser()
    {
        var resolution = Screen.Resolution;
        _width = Math.Max(1, resolution.Width);
        _height = Math.Max(1, resolution.Height);
        _browser = Browser.Create("liberty://ui/index.html", new BrowserOptions
        {
            OwnerId = OwnerId,
            Width = _width,
            Height = _height,
            X = 0,
            Y = 0,
            Fps = 60,
            Transparent = true,
            Visible = false,
            Focused = false,
            ZIndex = 900
        });
        _browser.On<object>("ui:ready", _ =>
        {
            _pageReady = true;
            _lastStateJson = null;
            SetView(_view, true);
            if (_pendingState != null)
            {
                _browser?.Emit("ui:state", _pendingState);
                _lastStateJson = _pendingState.ToString(Formatting.None);
            }
        });
        _browser.On<JObject>("ui:action", action => _actions.Enqueue(action));
    }

    private void SetView(string view, bool force = false)
    {
        if (!force && string.Equals(_view, view, StringComparison.Ordinal)) return;
        _view = view;
        if (_browser == null || !_pageReady) return;
        if (view == "hidden")
        {
            _browser.Unfocus();
            _browser.Hide();
        }
        else
        {
            _browser.Show();
            _browser.Focus();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _browser?.Dispose();
        _browser = null;
        try { Browser.DestroyOwnedBrowsers(OwnerId); } catch { }
    }
}
