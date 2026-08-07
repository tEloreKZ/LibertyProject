#pragma once

#include <filesystem>

namespace liberty {

class CefRuntime final {
 public:
  bool Initialize(const std::filesystem::path& game_root);
  void Shutdown();
  bool initialized() const { return initialized_; }

 private:
  bool initialized_{};
};

}  // namespace liberty

