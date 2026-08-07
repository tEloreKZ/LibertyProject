#include "InputManager.h"

#include "BrowserManager.h"
#include "include/base/cef_callback.h"
#include "include/cef_task.h"
#include "include/wrapper/cef_closure_task.h"

#include <windowsx.h>

namespace liberty {
namespace {

uint32_t Modifiers() {
  uint32_t value = 0;
  if (GetKeyState(VK_SHIFT) < 0) value |= EVENTFLAG_SHIFT_DOWN;
  if (GetKeyState(VK_CONTROL) < 0) value |= EVENTFLAG_CONTROL_DOWN;
  if (GetKeyState(VK_MENU) < 0) value |= EVENTFLAG_ALT_DOWN;
  if (GetKeyState(VK_CAPITAL) & 1) value |= EVENTFLAG_CAPS_LOCK_ON;
  if (GetKeyState(VK_NUMLOCK) & 1) value |= EVENTFLAG_NUM_LOCK_ON;
  if (GetKeyState(VK_LBUTTON) < 0) value |= EVENTFLAG_LEFT_MOUSE_BUTTON;
  if (GetKeyState(VK_RBUTTON) < 0) value |= EVENTFLAG_RIGHT_MOUSE_BUTTON;
  if (GetKeyState(VK_MBUTTON) < 0) value |= EVENTFLAG_MIDDLE_MOUSE_BUTTON;
  return value;
}

void PostMouseMove(const std::shared_ptr<Browser>& browser, const int x, const int y,
                   const bool leaving) {
  CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> cef_browser, int px, int py,
                                       uint32_t modifiers, bool leave) {
    if (!cef_browser) return;
    CefMouseEvent event; event.x = px; event.y = py; event.modifiers = modifiers;
    cef_browser->GetHost()->SendMouseMoveEvent(event, leave);
  }, browser->cef_browser(), x, y, Modifiers(), leaving));
}

void PostMouseClick(const std::shared_ptr<Browser>& browser, const int x, const int y,
                    const CefBrowserHost::MouseButtonType button, const bool mouse_up,
                    const int click_count) {
  CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> cef_browser, int px, int py,
                                       uint32_t modifiers, CefBrowserHost::MouseButtonType type,
                                       bool up, int count) {
    if (!cef_browser) return;
    CefMouseEvent event; event.x = px; event.y = py; event.modifiers = modifiers;
    cef_browser->GetHost()->SendMouseClickEvent(event, type, up, count);
  }, browser->cef_browser(), x, y, Modifiers(), button, mouse_up, click_count));
}

void PostKey(const std::shared_ptr<Browser>& browser, const CefKeyEvent& event) {
  CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> cef_browser, CefKeyEvent key) {
    if (cef_browser) cef_browser->GetHost()->SendKeyEvent(key);
  }, browser->cef_browser(), event));
}

}  // namespace

InputManager& InputManager::Instance() {
  static InputManager manager;
  return manager;
}

void InputManager::Attach(HWND window) {
  if (!window) return;
  std::scoped_lock lock(mutex_);
  if (window_ == window) return;
  if (window_ && original_procedure_)
    SetWindowLongPtrW(window_, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(original_procedure_));
  window_ = window;
  original_procedure_ = reinterpret_cast<WNDPROC>(SetWindowLongPtrW(
      window_, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(&WindowProcedure)));
}

void InputManager::Detach() {
  std::scoped_lock lock(mutex_);
  if (window_ && original_procedure_)
    SetWindowLongPtrW(window_, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(original_procedure_));
  window_ = nullptr;
  original_procedure_ = nullptr;
}

bool InputManager::IsCapturing() const {
  return BrowserManager::Instance().FocusedBrowser() != nullptr;
}

LRESULT CALLBACK InputManager::WindowProcedure(HWND window, const UINT message,
                                               const WPARAM wparam, const LPARAM lparam) {
  auto& manager = Instance();
  bool handled = false;
  const auto result = manager.Handle(window, message, wparam, lparam, handled);
  if (handled) return result;
  WNDPROC original{};
  {
    std::scoped_lock lock(manager.mutex_);
    original = manager.original_procedure_;
  }
  return original ? CallWindowProcW(original, window, message, wparam, lparam)
                  : DefWindowProcW(window, message, wparam, lparam);
}

LRESULT InputManager::Handle(HWND, const UINT message, const WPARAM wparam,
                             const LPARAM lparam, bool& handled) {
  const auto browser = BrowserManager::Instance().FocusedBrowser();
  if (!browser) return 0;
  const auto options = browser->options();
  const int x = GET_X_LPARAM(lparam) - options.x;
  const int y = GET_Y_LPARAM(lparam) - options.y;
  switch (message) {
    case WM_KILLFOCUS:
      PostMouseMove(browser, x, y, true);
      BrowserManager::Instance().Focus(browser->id(), false);
      return 0;
    case WM_CANCELMODE:
    case WM_CAPTURECHANGED:
      PostMouseMove(browser, x, y, true);
      return 0;
    case WM_MOUSEMOVE:
      PostMouseMove(browser, x, y, false); handled = true; return 0;
    case WM_MOUSELEAVE:
      PostMouseMove(browser, x, y, true); handled = true; return 0;
    case WM_LBUTTONDOWN: case WM_LBUTTONUP:
      PostMouseClick(browser, x, y, MBT_LEFT, message == WM_LBUTTONUP, 1);
      handled = true; return 0;
    case WM_RBUTTONDOWN: case WM_RBUTTONUP:
      PostMouseClick(browser, x, y, MBT_RIGHT, message == WM_RBUTTONUP, 1);
      handled = true; return 0;
    case WM_MBUTTONDOWN: case WM_MBUTTONUP:
      PostMouseClick(browser, x, y, MBT_MIDDLE, message == WM_MBUTTONUP, 1);
      handled = true; return 0;
    case WM_MOUSEWHEEL: {
      CefMouseEvent event; POINT point{GET_X_LPARAM(lparam), GET_Y_LPARAM(lparam)};
      ScreenToClient(window_, &point); event.x = point.x - options.x; event.y = point.y - options.y;
      event.modifiers = Modifiers();
      const int delta = GET_WHEEL_DELTA_WPARAM(wparam);
      CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> value, CefMouseEvent mouse,
                                           int wheel) {
        if (value) value->GetHost()->SendMouseWheelEvent(mouse, 0, wheel);
      }, browser->cef_browser(), event, delta));
      handled = true; return 0;
    }
    case WM_KEYDOWN: case WM_SYSKEYDOWN: case WM_KEYUP: case WM_SYSKEYUP: {
      CefKeyEvent event;
      event.type = (message == WM_KEYUP || message == WM_SYSKEYUP) ? KEYEVENT_KEYUP
                                                                  : KEYEVENT_RAWKEYDOWN;
      event.windows_key_code = static_cast<int>(wparam);
      event.native_key_code = static_cast<int>(lparam);
      event.modifiers = Modifiers();
      event.is_system_key = message == WM_SYSKEYDOWN || message == WM_SYSKEYUP;
      PostKey(browser, event); handled = true; return 0;
    }
    case WM_CHAR: case WM_SYSCHAR: {
      CefKeyEvent event; event.type = KEYEVENT_CHAR;
      event.windows_key_code = static_cast<int>(wparam);
      event.native_key_code = static_cast<int>(lparam);
      event.modifiers = Modifiers(); event.is_system_key = message == WM_SYSCHAR;
      PostKey(browser, event); handled = true; return 0;
    }
    case WM_INPUT:
      handled = true; return 0;
    default:
      return 0;
  }
}

}  // namespace liberty
