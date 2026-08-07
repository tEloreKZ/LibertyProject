#pragma once

#include <atomic>
#include <cstdint>
#include <memory>
#include <mutex>
#include <string>
#include <unordered_map>
#include <vector>

#include "Browser.h"

namespace liberty {

class BrowserManager final {
 public:
  static BrowserManager& Instance();

  void SetReady(bool ready);
  bool IsReady() const { return ready_.load(); }
  std::uint64_t CreateBrowser(BrowserOptions options);
  bool DestroyBrowser(std::uint64_t id);
  std::size_t DestroyBrowsersByOwner(const std::string& owner);
  bool Show(std::uint64_t id, bool visible);
  bool Focus(std::uint64_t id, bool focused);
  bool LoadUrl(std::uint64_t id, const std::string& url);
  bool SetBounds(std::uint64_t id, int x, int y, int width, int height);
  bool SetZIndex(std::uint64_t id, int z_index);
  bool Emit(std::uint64_t id, const std::string& event_name, const std::string& json);
  void Shutdown();

  std::shared_ptr<Browser> Find(std::uint64_t id) const;
  std::shared_ptr<Browser> FocusedBrowser() const;
  std::vector<std::shared_ptr<Browser>> VisibleBrowsers() const;
  void AttachCefBrowser(std::uint64_t id, CefRefPtr<CefBrowser> browser);
  void OnBrowserClosed(std::uint64_t id);
  void OnRendererEvent(std::uint64_t id, std::string name, std::string json);

 private:
  static bool IsAllowedUrl(const std::string& url);

  mutable std::mutex mutex_;
  std::unordered_map<std::uint64_t, std::shared_ptr<Browser>> browsers_;
  std::atomic<std::uint64_t> next_id_{1};
  std::atomic_bool ready_{false};
};

}  // namespace liberty

