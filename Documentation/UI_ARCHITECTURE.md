# React UI architecture

Project Liberty keeps gameplay state and GTA native calls on the game thread. All current and future user interfaces are implemented in React and TypeScript and rendered by the independent native CEF service documented in [CEF.md](CEF.md). Microsoft WebView2 and external overlay windows are not part of the runtime.

## Runtime flow

```text
React + TypeScript
    <-> window.liberty / CEF process IPC
    <-> native ProjectLiberty.Core BrowserManager
    <-> lifecycle-safe managed event queue
    <-> SHVDN or another managed plugin on the game thread
```

CEF belongs to `ProjectLiberty.Core.asi`, not to SHVDN. Reloading managed scripts does not restart Chromium. A managed browser wrapper owns only a native `BrowserId` and never owns the CEF runtime.

## Source layout

- `src/WebUI` contains the production Vite, React and TypeScript application.
- `ui/cef-test` contains the isolated native/managed round-trip test page.
- `src/LibertyScript/UI/CefUiBridge.cs` maps dashboard state and allow-listed actions to the managed Browser API.
- `src/LibertyScript/LibertyScript.cs` builds UI state and applies validated actions on the GTA game thread.
- `native/ProjectLiberty.Core/src/UI` and related CEF/graphics sources own browser lifecycle, input and rendering.

`build.bat` builds the production React bundle into `src/WebUI/dist` and deploys it to `GTA V/ProjectLiberty/ui`. The native `liberty://ui/index.html` scheme serves this directory without granting arbitrary filesystem access.

## Message contract

The managed layer sends `ui:state` with a JSON payload selecting `hidden`, `dashboard` or `editor` and carrying character/editor data. React emits `ui:action` with an allow-listed operation such as character selection or an editor adjustment. Page readiness is announced through `ui:ready`.

React cannot send arbitrary native hashes, model names, filesystem paths or executable commands. New screens should follow the same state/action pattern and expose only explicitly routed events through `window.liberty`.

## Display and input

CEF renders a transparent off-screen BGRA surface directly into GTA's swap chain. The native input manager forwards mouse, wheel, keyboard and text input only to the topmost focused browser. The layout uses responsive CSS and the browser is resized to the GTA client resolution, so no external overlay window or WebView2 Runtime is required.
