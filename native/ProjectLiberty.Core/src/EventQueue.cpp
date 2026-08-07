#include "EventQueue.h"

#include <algorithm>

namespace liberty {

EventQueue& EventQueue::Instance() {
  static EventQueue queue;
  return queue;
}

void EventQueue::Push(BrowserEvent event) {
  std::scoped_lock lock(mutex_);
  if (events_.size() >= kMaximumEvents) events_.pop_front();
  events_.push_back(std::move(event));
}

std::optional<BrowserEvent> EventQueue::Peek() const {
  std::scoped_lock lock(mutex_);
  if (events_.empty()) return std::nullopt;
  return events_.front();
}

std::optional<BrowserEvent> EventQueue::Pop() {
  std::scoped_lock lock(mutex_);
  if (events_.empty()) return std::nullopt;
  BrowserEvent event = std::move(events_.front());
  events_.pop_front();
  return event;
}

void EventQueue::RemoveBrowser(const std::uint64_t browser_id) {
  std::scoped_lock lock(mutex_);
  std::erase_if(events_, [browser_id](const BrowserEvent& event) {
    return event.browser_id == browser_id;
  });
}

void EventQueue::Clear() {
  std::scoped_lock lock(mutex_);
  events_.clear();
}

}  // namespace liberty

