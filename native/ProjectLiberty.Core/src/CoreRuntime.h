#pragma once

#include <Windows.h>

#include <atomic>
#include <filesystem>

#include "Cef/CefRuntime.h"
#include "Graphics/DxgiHook.h"

namespace liberty {

class CoreRuntime final {
 public:
  static CoreRuntime& Instance();

  bool Start(HMODULE module);
  void RequestStop();
  bool IsReady() const { return ready_.load(); }
  DWORD Run();

 private:
  HMODULE module_{};
  HANDLE stop_event_{};
  HANDLE worker_{};
  DLL_DIRECTORY_COOKIE dll_directory_cookie_{};
  std::filesystem::path game_root_;
  std::atomic_bool ready_{false};
  CefRuntime cef_runtime_;
  DxgiHook graphics_hook_;
};

}  // namespace liberty

