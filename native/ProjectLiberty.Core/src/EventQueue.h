#pragma once

#include <cstdint>
#include <deque>
#include <mutex>
#include <optional>
#include <string>

namespace liberty {

struct BrowserEvent final {
  std::uint64_t browser_id{};
  std::string name;
  std::string json;
};

class EventQueue final {
 public:
  static EventQueue& Instance();

  bool Push(BrowserEvent event);
  std::optional<BrowserEvent> Peek() const;
  std::optional<BrowserEvent> Pop();
  void RemoveBrowser(std::uint64_t browser_id);
  void Clear();

 private:
  static constexpr std::size_t kMaximumEvents = 1024;
  static constexpr std::size_t kMaximumBytes = 16 * 1024 * 1024;
  mutable std::mutex mutex_;
  std::deque<BrowserEvent> events_;
  std::size_t queued_bytes_{};
};

}  // namespace liberty
