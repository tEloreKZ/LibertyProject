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

  void Push(BrowserEvent event);
  std::optional<BrowserEvent> Peek() const;
  std::optional<BrowserEvent> Pop();
  void RemoveBrowser(std::uint64_t browser_id);
  void Clear();

 private:
  static constexpr std::size_t kMaximumEvents = 4096;
  mutable std::mutex mutex_;
  std::deque<BrowserEvent> events_;
};

}  // namespace liberty

