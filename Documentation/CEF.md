# Native CEF service

## Purpose

Project Liberty embeds Chromium Embedded Framework as an independent native GTA V subsystem. `ProjectLiberty.Core.asi` owns CEF, browser processes, rendering, input and the JavaScript bridge. ScriptHookVDotNet and third-party managed plugins are clients of the stable C ABI through `ProjectLiberty.Managed.dll`; unloading or reloading a managed script does not shut down CEF.

```text
GTA V / ASI loader
  -> ProjectLiberty.Core.asi
       -> CEF runtime
       -> BrowserManager
       -> DXGI Present hook -> D3D11 / D3D11-on-12 backend
       -> GTA WndProc -> InputManager
       -> ProjectLiberty.CefSubprocess.exe
  -> ScriptHookVDotNet
       -> ProjectLiberty.Managed.dll
       -> C ABI + lifecycle-safe event polling
```

## Source layout

- `native/ProjectLiberty.Core` contains the ASI, subprocess, CEF integration, browser manager, input and graphics backend.
- `native/ProjectLiberty.Core/include/ProjectLiberty/BrowserApi.h` is the versioned C ABI.
- `src/Managed/UI` contains the public .NET browser wrapper.
- `src/LibertyScript/UI/CefTestController.cs` is the SHVDN round-trip test client.
- `ui/cef-test` is the React + TypeScript source for the transparent test page served through `liberty://`; Vite emits its deployable `dist` bundle.
- `Build/Setup-Cef.ps1` downloads the pinned official CEF binary distribution.
- `Build/Build-Native.ps1` generates and builds the x64 native projects.

## Lifecycle and ownership

The ASI loader loads `ProjectLiberty.Core.asi` from the GTA V root. A native worker initializes logging, adds `ProjectLiberty/Native` to the DLL search path, initializes CEF and installs the DXGI hooks. CEF uses its multi-threaded message loop and a separate subprocess. On Windows, CEF 150 uses the official `bootstrap.exe`, deployed as `ProjectLiberty.CefSubprocess.exe`; the bootstrap loads the same-basename client module `ProjectLiberty.CefSubprocess.dll`. The runtime deployment also includes the mandatory `v8_context_snapshot.bin`.

The production browser is created lazily on the first F2 press. Loading Story Mode initializes the native service but does not create a renderer or network-service process merely for a hidden page.

A managed `Browser` owns only a `BrowserId`. It never owns CEF. Each browser also has an owner string. `DestroyOwnedBrowsers` supports deterministic cleanup when a plugin or SHVDN script unloads; native CEF itself remains alive. Native-to-managed events are copied into a thread-safe queue, then polled on the SHVDN game tick, so CEF callbacks never execute GTA natives from a CEF thread and no native function pointer targets an unloaded delegate.

At GTA shutdown the native core removes input/render hooks, closes browsers, clears scheme handlers and shuts down CEF. Renderer termination is logged and recovery is limited to three attempts per 30 seconds; after that automatic reload stops so a failed renderer cannot create an FPS-destroying crash loop.

## Rendering

CEF browsers are created windowless with transparency enabled. The initial stable path is:

```text
CEF OnPaint BGRA buffer
  -> immutable CPU frame
  -> D3D11 BGRA texture
  -> premultiplied-alpha composition during IDXGISwapChain::Present
```

`IGraphicsBackend` separates browser management from the graphics API. Legacy GTA V uses the direct D3D11 path. GTA V Enhanced captures its D3D12 command queue and creates an official D3D11-on-12 interop device, allowing the same compositor to draw into wrapped D3D12 swap-chain buffers. Dirty rectangles are copied and uploaded independently instead of transferring the complete browser surface for small changes.

The DXGI hook is installed once by native core. Resize invalidates the render target and browser bounds notify CEF through `WasResized`.

## Input

On the first GTA swap-chain presentation, `InputManager` subclasses the game window procedure. Only the topmost visible focused browser receives mouse movement, left/right/middle buttons, wheel, key down/up and UTF-16 character input. Messages handled by a focused browser are consumed so the game does not also react. Hidden or unfocused browsers receive no input.

This layer is completely native and has no SHVDN dependency.

## JavaScript bridge

Every allowed page receives a deliberately small API:

```ts
window.liberty.emit("character:create", { firstName: "Michael" });

const callback = (player: { health: number }) => console.log(player.health);
window.liberty.on("player:update", callback);
window.liberty.off("player:update", callback);
```

`emit` serializes JSON and sends a `CefProcessMessage` to the browser process. Events sent by native or managed code use the reverse IPC direction and invoke registered callbacks after `JSON.parse`. The implementation does not use `eval` and exposes no filesystem, GTA native or process access.

## Managed API

```csharp
using ProjectLiberty.UI;

using var browser = Browser.Create("liberty://ui/cef-test/index.html",
    new BrowserOptions { Width = 760, Height = 460, Focused = true });

browser.On<TestMessage>("cef:test", value =>
    browser.Emit("cef:test-response", new { message = value.Message }));

// Call on the owning game/main thread once per tick.
Browser.DispatchPendingEvents();

browser.Show();
browser.Hide();
browser.Focus();
browser.Unfocus();
browser.LoadUrl("liberty://ui/another/index.html");
```

`BrowserOptions` also supports owner, position, FPS, transparency, initial visibility and z-index. `Browser` is `IDisposable`. Plugins should use a stable, unique owner id and call `Browser.DestroyOwnedBrowsers(ownerId)` in their unload path.

## URL scheme and security boundary

Absolute `file://` URLs are rejected. The native scheme handler accepts only:

- `liberty://ui/<path>` from `GTA V/ProjectLiberty/ui/<path>`;
- `liberty://mods/<mod-name>/<path>` from `GTA V/ProjectLiberty/mods/<mod-name>/ui/<path>`.

Paths are canonicalized and must stay below the selected root, preventing `..` traversal. Only a MIME allow-list is served. This is the foundation for a future per-mod permission system; it is not a full sandbox for trusted managed plugins.

## Dependencies and build

Required tools are Windows x64, Visual Studio C++ build tools with CMake, .NET 8 SDK, .NET Framework 4.8 targeting pack and Node.js 20+.

The native build pins:

- CEF `150.0.10+g8042e43+chromium-150.0.7871.101`, Windows x64 minimal distribution;
- MinHook `1.3.4`, fetched by CMake with a pinned SHA-256.

CEF is downloaded from the official automated-build host into the ignored `third_party/cef` directory:

```powershell
powershell -ExecutionPolicy Bypass -File Build/Setup-Cef.ps1
powershell -ExecutionPolicy Bypass -File Build/Build-Native.ps1 -Configuration Release
```

For the complete project run `build.bat`. The installable tree is produced at `output/gta-root`.

## Deployment

Copy the contents of `output/gta-root` into the GTA V root, preserving this layout:

```text
GTA V/
  ProjectLiberty.Core.asi
  ProjectLiberty/
    Native/
      ProjectLiberty.CefSubprocess.exe
      ProjectLiberty.CefSubprocess.dll
      libcef.dll
      chrome_elf.dll
      v8_context_snapshot.bin
      resources.pak and other CEF runtime files
      locales/
    ui/cef-test/
    Logs/
    Cache/CEF/
  scripts/
    LibertyScript.dll
    ProjectLiberty/Core/ProjectLiberty.Managed.dll
```

An ASI loader, ScriptHookV and ScriptHookVDotNet 3.x must be installed. Microsoft Edge WebView2 is not used or required.

## Round-trip test

1. Build and deploy the complete `output/gta-root` tree.
2. Start GTA V and load Story Mode.
3. Press F10. A transparent `PROJECT LIBERTY CEF TEST` browser should appear.
4. Move and click with the mouse and type into the text field.
5. Select **Send event to game**.
6. The page emits `cef:test`; SHVDN receives it on its tick and replies with `cef:test-response`; the page displays the player health and timestamp.
7. Press F10 again to test hide/unfocus and reopening.
8. Reload SHVDN scripts. The native core and CEF process must stay alive; the script-owned test browser is cleaned up and can be recreated.

Diagnostics are written to `ProjectLiberty/Logs/cef.log` and CEF warnings to `ProjectLiberty/Logs/cef-runtime.log`. The official x64 bootstrap provides the process startup contract required by CEF 150. Do not omit `ProjectLiberty.CefSubprocess.dll` or `v8_context_snapshot.bin` when deploying the native runtime.

## Current limitations

- The MVP uses CPU `OnPaint` uploads; accelerated shared textures are prepared architecturally but not implemented.
- D3D11 and D3D12 through D3D11-on-12 are supported. Vulkan is not currently supported.
- Input interception is Win32-message based. GTA/controller-specific action blocking and IME composition need further in-game validation.
- Browser ownership is enforced for cleanup, but a full per-plugin permission and quota manager is deferred.
- Crash recovery is bounded; restoration of application UI state remains the page/plugin's responsibility.
- Automated builds validate compilation and managed logic; presentation, alpha blending and GTA shutdown require an in-game smoke test on the target GTA/ScriptHook versions.

## Recommended next steps

1. Run an in-game matrix for fullscreen modes, resize, alt-tab, DPI, keyboard layouts and SHVDN reload.
2. Add timing telemetry for CEF paint, texture upload and composition to support hardware-specific tuning.
3. Add owner permissions, browser/event quotas and mod manifests.
4. Implement `OnAcceleratedPaint` behind `IGraphicsBackend` after the optimized dirty-rectangle CPU path is proven stable.
5. Add automated native browser lifecycle and IPC integration tests around the CEF test page.
