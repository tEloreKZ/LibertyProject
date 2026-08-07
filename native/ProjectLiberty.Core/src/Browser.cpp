#include "Browser.h"

#include <algorithm>
#include <cstring>

namespace liberty {

Browser::Browser(const std::uint64_t id, BrowserOptions options)
    : id_(id), options_(std::move(options)) {}

BrowserOptions Browser::options() const {
  std::scoped_lock lock(mutex_);
  return options_;
}

void Browser::SetBounds(const int x, const int y, const int width, const int height) {
  std::scoped_lock lock(mutex_);
  options_.x = x;
  options_.y = y;
  options_.width = std::max(1, width);
  options_.height = std::max(1, height);
}

void Browser::SetVisible(const bool value) {
  std::scoped_lock lock(mutex_);
  options_.visible = value;
}

void Browser::SetFocused(const bool value) {
  std::scoped_lock lock(mutex_);
  options_.focused = value;
}

void Browser::SetZIndex(const int value) {
  std::scoped_lock lock(mutex_);
  options_.z_index = value;
}

void Browser::SetCefBrowser(CefRefPtr<CefBrowser> browser) {
  std::scoped_lock lock(mutex_);
  cef_browser_ = std::move(browser);
}

CefRefPtr<CefBrowser> Browser::cef_browser() const {
  std::scoped_lock lock(mutex_);
  return cef_browser_;
}

void Browser::UpdateFrame(const void* buffer, const int width, const int height,
                          const std::vector<BrowserFrameRect>& dirty_rects) {
  if (!buffer || width <= 0 || height <= 0) return;
  std::scoped_lock lock(mutex_);
  const auto size = static_cast<std::size_t>(width) * height * 4;
  const bool resized = !frame_ || frame_->width != width || frame_->height != height;

  std::vector<BrowserFrameRect> current_dirty_rects;
  current_dirty_rects.reserve(dirty_rects.size());
  for (const auto& input : dirty_rects) {
    const int left = std::clamp(input.x, 0, width);
    const int top = std::clamp(input.y, 0, height);
    const int right = std::clamp(input.x + input.width, left, width);
    const int bottom = std::clamp(input.y + input.height, top, height);
    if (left != right && top != bottom)
      current_dirty_rects.push_back({left, top, right - left, bottom - top});
  }

  if (resized) {
    frame_ = std::make_shared<BrowserFrame>();
    frame_->width = width;
    frame_->height = height;
    frame_->bgra.resize(size);
    std::memcpy(frame_->bgra.data(), buffer, size);
    frame_->dirty_rects = {{0, 0, width, height}};
  } else {
    frame_->dirty_rects.clear();
    const auto* source = static_cast<const std::uint8_t*>(buffer);
    auto upload_rects = current_dirty_rects;
    upload_rects.insert(upload_rects.end(), previous_dirty_rects_.begin(),
                        previous_dirty_rects_.end());
    for (const auto& rect : upload_rects) {
      const int left = rect.x;
      const int top = rect.y;
      const int right = rect.x + rect.width;
      const int bottom = rect.y + rect.height;
      for (int row = top; row < bottom; ++row) {
        const auto offset = (static_cast<std::size_t>(row) * width + left) * 4;
        std::memcpy(frame_->bgra.data() + offset, source + offset,
                    static_cast<std::size_t>(right - left) * 4);
      }
      frame_->dirty_rects.push_back({left, top, right - left, bottom - top});
    }
  }
  previous_dirty_rects_ = std::move(current_dirty_rects);
  frame_->sequence = ++next_sequence_;
}

void Browser::WithFrame(const std::function<void(const BrowserFrame&)>& reader) const {
  if (!reader) return;
  std::scoped_lock lock(mutex_);
  if (frame_) reader(*frame_);
}

}  // namespace liberty
