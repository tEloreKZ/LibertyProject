#pragma once

#include <Windows.h>

#include <mutex>

namespace liberty {

class InputManager final {
 public:
  static InputManager& Instance();

  void Attach(HWND window);
  void Detach();
  bool IsCapturing() const;

 private:
  static LRESULT CALLBACK WindowProcedure(HWND window, UINT message, WPARAM wparam, LPARAM lparam);
  LRESULT Handle(HWND window, UINT message, WPARAM wparam, LPARAM lparam, bool& handled);

  mutable std::mutex mutex_;
  HWND window_{};
  WNDPROC original_procedure_{};
};

}  // namespace liberty

