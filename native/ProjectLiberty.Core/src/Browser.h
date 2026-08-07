#pragma once

#include <atomic>
#include <cstdint>
#include <functional>
#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include "include/cef_browser.h"

namespace liberty {

struct BrowserOptions final {
  std::string url;
  std::string owner;
  int width{1920};
  int height{1080};
  int x{};
  int y{};
  int fps{60};
  bool transparent{true};
  bool visible{true};
  bool focused{false};
  int z_index{};
};

struct BrowserFrameRect final {
  int x{};
  int y{};
  int width{};
  int height{};
};

struct BrowserFrame final {
  int width{};
  int height{};
  std::uint64_t sequence{};
  std::vector<std::uint8_t> bgra;
  std::vector<BrowserFrameRect> dirty_rects;
};

class Browser final {
 public:
  Browser(std::uint64_t id, BrowserOptions options);

  std::uint64_t id() const { return id_; }
  BrowserOptions options() const;
  void SetBounds(int x, int y, int width, int height);
  void SetVisible(bool value);
  void SetFocused(bool value);
  void SetZIndex(int value);

  void SetCefBrowser(CefRefPtr<CefBrowser> browser);
  CefRefPtr<CefBrowser> cef_browser() const;
  void UpdateFrame(const void* buffer, int width, int height,
                   const std::vector<BrowserFrameRect>& dirty_rects);
  void WithFrame(const std::function<void(const BrowserFrame&)>& reader) const;

 private:
  const std::uint64_t id_;
  mutable std::mutex mutex_;
  BrowserOptions options_;
  CefRefPtr<CefBrowser> cef_browser_;
  std::shared_ptr<BrowserFrame> frame_;
  std::vector<BrowserFrameRect> previous_dirty_rects_;
  std::uint64_t next_sequence_{};
};

}  // namespace liberty
