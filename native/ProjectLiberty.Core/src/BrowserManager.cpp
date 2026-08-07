#include "BrowserManager.h"

#include <algorithm>

#include "Cef/CefClient.h"
#include "EventQueue.h"
#include "Logging/NativeLogger.h"
#include "include/base/cef_callback.h"
#include "include/cef_parser.h"
#include "include/cef_task.h"
#include "include/wrapper/cef_closure_task.h"

namespace liberty {

BrowserManager& BrowserManager::Instance() {
  static BrowserManager manager;
  return manager;
}

void BrowserManager::SetReady(const bool ready) { ready_.store(ready); }

std::uint64_t BrowserManager::CreateBrowser(BrowserOptions options) {
  if (!IsReady() || !IsAllowedUrl(options.url)) return 0;
  options.width = std::max(1, options.width);
  options.height = std::max(1, options.height);
  options.fps = std::clamp(options.fps, 1, 120);
  const auto id = next_id_.fetch_add(1);
  auto state = std::make_shared<Browser>(id, std::move(options));
  {
    std::scoped_lock lock(mutex_);
    browsers_.emplace(id, state);
  }

  CefPostTask(TID_UI, base::BindOnce([](std::shared_ptr<Browser> browser) {
    const auto options = browser->options();
    CefWindowInfo window_info;
    window_info.SetAsWindowless(nullptr);
    window_info.shared_texture_enabled = false;
    CefBrowserSettings settings;
    settings.windowless_frame_rate = options.fps;
    settings.background_color = options.transparent ? CefColorSetARGB(0, 0, 0, 0)
                                                    : CefColorSetARGB(255, 0, 0, 0);
    auto client = new LibertyCefClient(browser);
    if (!CefBrowserHost::CreateBrowser(window_info, client, options.url, settings,
                                       nullptr, nullptr)) {
      NativeLogger::Instance().Error("[CEF] Failed to create browser #" +
                                     std::to_string(browser->id()));
      BrowserManager::Instance().OnBrowserClosed(browser->id());
    }
  }, state));
  NativeLogger::Instance().Info("[CEF] Creating browser #" + std::to_string(id));
  return id;
}

bool BrowserManager::DestroyBrowser(const std::uint64_t id) {
  std::shared_ptr<Browser> browser;
  {
    std::scoped_lock lock(mutex_);
    const auto iterator = browsers_.find(id);
    if (iterator == browsers_.end()) return false;
    browser = iterator->second;
    browsers_.erase(iterator);
  }
  EventQueue::Instance().RemoveBrowser(id);
  auto cef_browser = browser->cef_browser();
  if (cef_browser) {
    CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> value) {
      if (value && value->GetHost()) value->GetHost()->CloseBrowser(true);
    }, cef_browser));
  }
  return true;
}

std::size_t BrowserManager::DestroyBrowsersByOwner(const std::string& owner) {
  std::vector<std::uint64_t> ids;
  {
    std::scoped_lock lock(mutex_);
    for (const auto& [id, browser] : browsers_) {
      if (browser->options().owner == owner) ids.push_back(id);
    }
  }
  for (const auto id : ids) DestroyBrowser(id);
  return ids.size();
}

bool BrowserManager::Show(const std::uint64_t id, const bool visible) {
  auto browser = Find(id);
  if (!browser) return false;
  browser->SetVisible(visible);
  auto cef_browser = browser->cef_browser();
  if (cef_browser) CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> value, bool hidden) {
    value->GetHost()->WasHidden(hidden);
  }, cef_browser, !visible));
  return true;
}

bool BrowserManager::Focus(const std::uint64_t id, const bool focused) {
  auto target = Find(id);
  if (!target) return false;
  std::vector<std::shared_ptr<Browser>> snapshot;
  {
    std::scoped_lock lock(mutex_);
    for (const auto& [_, browser] : browsers_) snapshot.push_back(browser);
  }
  for (const auto& browser : snapshot) {
    const bool value = focused && browser->id() == id;
    browser->SetFocused(value);
    auto cef_browser = browser->cef_browser();
    if (cef_browser) CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> item, bool focus) {
      item->GetHost()->SetFocus(focus);
    }, cef_browser, value));
  }
  return true;
}

bool BrowserManager::LoadUrl(const std::uint64_t id, const std::string& url) {
  if (!IsAllowedUrl(url)) return false;
  auto browser = Find(id);
  if (!browser) return false;
  auto cef_browser = browser->cef_browser();
  if (!cef_browser) return false;
  CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> value, std::string target) {
    if (auto frame = value->GetMainFrame()) frame->LoadURL(target);
  }, cef_browser, url));
  return true;
}

bool BrowserManager::SetBounds(const std::uint64_t id, const int x, const int y,
                               const int width, const int height) {
  auto browser = Find(id);
  if (!browser || width <= 0 || height <= 0) return false;
  browser->SetBounds(x, y, width, height);
  auto cef_browser = browser->cef_browser();
  if (cef_browser) CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> value) {
    value->GetHost()->WasResized();
  }, cef_browser));
  return true;
}

bool BrowserManager::SetZIndex(const std::uint64_t id, const int z_index) {
  auto browser = Find(id);
  if (!browser) return false;
  browser->SetZIndex(z_index);
  return true;
}

bool BrowserManager::Emit(const std::uint64_t id, const std::string& event_name,
                          const std::string& json) {
  if (event_name.empty() || !CefParseJSON(json, JSON_PARSER_RFC)) return false;
  auto browser = Find(id);
  if (!browser) return false;
  auto cef_browser = browser->cef_browser();
  if (!cef_browser) return false;
  CefPostTask(TID_UI, base::BindOnce([](CefRefPtr<CefBrowser> value, std::string name,
                                       std::string payload) {
    auto message = CefProcessMessage::Create("liberty.event");
    message->GetArgumentList()->SetString(0, name);
    message->GetArgumentList()->SetString(1, payload);
    if (auto frame = value->GetMainFrame()) frame->SendProcessMessage(PID_RENDERER, message);
  }, cef_browser, event_name, json));
  return true;
}

void BrowserManager::Shutdown() {
  std::vector<std::uint64_t> ids;
  {
    std::scoped_lock lock(mutex_);
    for (const auto& [id, _] : browsers_) ids.push_back(id);
  }
  for (const auto id : ids) DestroyBrowser(id);
  EventQueue::Instance().Clear();
  ready_.store(false);
}

std::shared_ptr<Browser> BrowserManager::Find(const std::uint64_t id) const {
  std::scoped_lock lock(mutex_);
  const auto iterator = browsers_.find(id);
  return iterator == browsers_.end() ? nullptr : iterator->second;
}

std::shared_ptr<Browser> BrowserManager::FocusedBrowser() const {
  auto values = VisibleBrowsers();
  for (auto iterator = values.rbegin(); iterator != values.rend(); ++iterator) {
    if ((*iterator)->options().focused) return *iterator;
  }
  return nullptr;
}

std::vector<std::shared_ptr<Browser>> BrowserManager::VisibleBrowsers() const {
  std::vector<std::shared_ptr<Browser>> values;
  {
    std::scoped_lock lock(mutex_);
    for (const auto& [_, browser] : browsers_) {
      if (browser->options().visible) values.push_back(browser);
    }
  }
  std::ranges::sort(values, {}, [](const std::shared_ptr<Browser>& browser) {
    return browser->options().z_index;
  });
  return values;
}

void BrowserManager::AttachCefBrowser(const std::uint64_t id, CefRefPtr<CefBrowser> browser) {
  auto state = Find(id);
  if (!state) {
    browser->GetHost()->CloseBrowser(true);
    return;
  }
  state->SetCefBrowser(browser);
  const auto options = state->options();
  browser->GetHost()->SetWindowlessFrameRate(options.fps);
  browser->GetHost()->WasHidden(!options.visible);
  browser->GetHost()->SetFocus(options.focused);
}

void BrowserManager::OnBrowserClosed(const std::uint64_t id) {
  std::scoped_lock lock(mutex_);
  browsers_.erase(id);
  NativeLogger::Instance().Info("[CEF] Browser #" + std::to_string(id) + " closed");
}

void BrowserManager::OnRendererEvent(const std::uint64_t id, std::string name,
                                     std::string json) {
  if (!Find(id) || name.empty() || name.size() > 128 || json.size() > 16 * 1024 * 1024 ||
      !CefParseJSON(json, JSON_PARSER_RFC)) return;
  EventQueue::Instance().Push({id, std::move(name), std::move(json)});
}

bool BrowserManager::IsAllowedUrl(const std::string& url) {
  return url.starts_with("liberty://ui/") || url.starts_with("liberty://mods/");
}

}  // namespace liberty
