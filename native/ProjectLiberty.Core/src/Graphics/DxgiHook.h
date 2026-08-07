#pragma once

#include <atomic>

namespace liberty {

class DxgiHook final {
 public:
  bool Initialize();
  void Shutdown();
  bool initialized() const { return initialized_.load(); }

 private:
  std::atomic_bool initialized_{false};
};

}  // namespace liberty

