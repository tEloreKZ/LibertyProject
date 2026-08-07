using GTA;
using GTA.UI;
using ProjectLiberty.UI;

namespace ProjectLiberty.LibertyScript.UI;

internal sealed class CefTestController : IDisposable
{
    private const string OwnerId = "project-liberty.shvdn.cef-test";
    private Browser? _browser;
    private bool _openRequested;
    private bool _visible;
    private bool _errorShown;

    public void Tick()
    {
        if (_browser == null && !_openRequested) return;
        if (Browser.IsNativeCoreReady)
        {
            try { Browser.DispatchPendingEvents(); }
            catch (Exception exception) { ShowError(exception); }
            if (_openRequested && _browser == null) CreateBrowser();
        }
    }

    public void Toggle()
    {
        if (_browser == null)
        {
            _openRequested = true;
            if (Browser.IsNativeCoreReady) CreateBrowser();
            else Notification.Show("~y~Project Liberty CEF~w~: native core ещё запускается");
            return;
        }
        try
        {
            if (_visible)
            {
                _browser.Unfocus();
                _browser.Hide();
                _visible = false;
            }
            else
            {
                _browser.Show();
                _browser.Focus();
                _visible = true;
            }
        }
        catch (Exception exception) { ShowError(exception); }
    }

    private void CreateBrowser()
    {
        try
        {
            _browser = Browser.Create("liberty://ui/cef-test/index.html", new BrowserOptions
            {
                OwnerId = OwnerId,
                Width = 760,
                Height = 460,
                X = 60,
                Y = 60,
                Fps = 60,
                Transparent = true,
                Visible = true,
                Focused = true,
                ZIndex = 1000
            });
            _browser.On<CefTestRequest>("cef:test", request =>
            {
                _browser?.Emit("cef:test-response", new
                {
                    message = $"Native/Managed round-trip: {request.Message}",
                    playerHealth = Game.Player.Character.Health,
                    utc = DateTime.UtcNow.ToString("O")
                });
            });
            _browser.On<object>("cef:close", _ =>
            {
                var browser = _browser;
                _browser = null;
                _visible = false;
                _openRequested = false;
                browser?.Dispose();
            });
            _visible = true;
            _openRequested = false;
            Notification.Show("~g~Project Liberty CEF~w~: тестовый браузер создан");
        }
        catch (Exception exception) { ShowError(exception); }
    }

    private void ShowError(Exception exception)
    {
        if (_errorShown) return;
        _errorShown = true;
        Notification.Show($"~r~Project Liberty CEF: {exception.Message}");
    }

    public void Dispose()
    {
        _browser?.Dispose();
        _browser = null;
        try { Browser.DestroyOwnedBrowsers(OwnerId); }
        catch { }
    }

    private sealed class CefTestRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
