#include "EventQueue.h"

#include <algorithm>

namespace liberty {

EventQueue& EventQueue::Instance() {
  static EventQueue queue;
  return queue;
}

bool EventQueue::Push(BrowserEvent event) {
  std::scoped_lock lock(mutex_);
  const auto event_bytes = event.name.size() + event.json.size();
  if (event_bytes > kMaximumBytes) return false;
  while (!events_.empty() &&
         (events_.size() >= kMaximumEvents || queued_bytes_ + event_bytes > kMaximumBytes)) {
    queued_bytes_ -= events_.front().name.size() + events_.front().json.size();
    events_.pop_front();
  }
  queued_bytes_ += event_bytes;
  events_.push_back(std::move(event));
  return true;
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
  queued_bytes_ -= event.name.size() + event.json.size();
  events_.pop_front();
  return event;
}

void EventQueue::RemoveBrowser(const std::uint64_t browser_id) {
  std::scoped_lock lock(mutex_);
  for (auto iterator = events_.begin(); iterator != events_.end();) {
    if (iterator->browser_id == browser_id) {
      queued_bytes_ -= iterator->name.size() + iterator->json.size();
      iterator = events_.erase(iterator);
    } else ++iterator;
  }
}

void EventQueue::Clear() {
  std::scoped_lock lock(mutex_);
  events_.clear();
  queued_bytes_ = 0;
}

}  // namespace liberty
