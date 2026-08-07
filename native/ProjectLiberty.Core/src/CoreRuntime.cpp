#include "CoreRuntime.h"

#include "Logging/NativeLogger.h"

namespace liberty {
namespace {

DWORD WINAPI RuntimeThread(void*) { return CoreRuntime::Instance().Run(); }

}  // namespace

CoreRuntime& CoreRuntime::Instance() {
  static CoreRuntime runtime;
  return runtime;
}

bool CoreRuntime::Start(HMODULE module) {
  if (worker_) return true;
  module_ = module;
  stop_event_ = CreateEventW(nullptr, TRUE, FALSE, nullptr);
  if (!stop_event_) return false;
  worker_ = CreateThread(nullptr, 0, RuntimeThread, nullptr, 0, nullptr);
  return worker_ != nullptr;
}

void CoreRuntime::RequestStop() {
  ready_.store(false);
  if (stop_event_) SetEvent(stop_event_);
}

DWORD CoreRuntime::Run() {
  wchar_t module_path[MAX_PATH]{};
  if (!GetModuleFileNameW(module_, module_path, MAX_PATH)) return 1;
  game_root_ = std::filesystem::path(module_path).parent_path();
  const auto project_root = game_root_ / L"ProjectLiberty";
  const auto native_root = project_root / L"Native";
  NativeLogger::Instance().Initialize(project_root / L"Logs");
  NativeLogger::Instance().Info("Native ProjectLiberty.Core starting");

  SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LOAD_LIBRARY_SEARCH_USER_DIRS);
  dll_directory_cookie_ = AddDllDirectory(native_root.c_str());
  SetDllDirectoryW(native_root.c_str());
  if (!graphics_hook_.Initialize()) {
    NativeLogger::Instance().Error("Native core stopped because DXGI initialization failed");
    return 3;
  }
  if (!cef_runtime_.Initialize(game_root_)) {
    NativeLogger::Instance().Error("Native core stopped because CEF initialization failed");
    graphics_hook_.Shutdown();
    return 2;
  }
  ready_.store(true);
  NativeLogger::Instance().Info("Native ProjectLiberty.Core ready");
  WaitForSingleObject(stop_event_, INFINITE);
  ready_.store(false);
  graphics_hook_.Shutdown();
  cef_runtime_.Shutdown();
  if (dll_directory_cookie_) RemoveDllDirectory(dll_directory_cookie_);
  SetDllDirectoryW(nullptr);
  NativeLogger::Instance().Info("Native ProjectLiberty.Core stopped");
  return 0;
}

}  // namespace liberty
