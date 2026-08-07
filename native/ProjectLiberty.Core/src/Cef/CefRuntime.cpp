#include "Cef/CefRuntime.h"

#include <Windows.h>

#include <thread>

#include "BrowserManager.h"
#include "Cef/CefApp.h"
#include "Cef/CefClient.h"
#include "Cef/CefSchemeHandler.h"
#include "Logging/NativeLogger.h"
#include "include/cef_app.h"
#include "include/cef_scheme.h"
#include "include/cef_task.h"
#include "include/wrapper/cef_closure_task.h"

namespace liberty {
namespace {

class SignalEventTask final : public CefTask {
 public:
  explicit SignalEventTask(HANDLE event) : event_(event) {}
  void Execute() override { SetEvent(event_); }

 private:
  HANDLE event_;
  IMPLEMENT_REFCOUNTING(SignalEventTask);
  DISALLOW_COPY_AND_ASSIGN(SignalEventTask);
};

}  // namespace

bool CefRuntime::Initialize(const std::filesystem::path& game_root) {
  if (initialized_) return true;
  auto& logger = NativeLogger::Instance();
  logger.Info("[CEF] Initializing");
  const auto native_root = game_root / L"ProjectLiberty" / L"Native";
  const auto content_root = game_root / L"ProjectLiberty";

  CefMainArgs arguments(GetModuleHandleW(nullptr));
  CefSettings settings;
  settings.no_sandbox = true;
  settings.windowless_rendering_enabled = true;
  settings.multi_threaded_message_loop = true;
  settings.background_color = CefColorSetARGB(0, 0, 0, 0);
  settings.log_severity = LOGSEVERITY_WARNING;
  CefString(&settings.browser_subprocess_path) =
      (native_root / L"ProjectLiberty.CefSubprocess.exe").wstring();
  CefString(&settings.resources_dir_path) = native_root.wstring();
  CefString(&settings.locales_dir_path) = (native_root / L"locales").wstring();
  CefString(&settings.cache_path) =
      (content_root / L"Cache" / L"CEF").wstring();
  CefString(&settings.log_file) =
      (content_root / L"Logs" / L"cef-runtime.log").wstring();

  auto application = new LibertyCefApp();
  if (!CefInitialize(arguments, settings, application, nullptr)) {
    logger.Error("[CEF] Initialization failed with code " +
                 std::to_string(CefGetExitCode()));
    return false;
  }
  CefRegisterSchemeHandlerFactory("liberty", "", new LibertySchemeHandlerFactory(content_root));
  BrowserManager::Instance().SetReady(true);
  initialized_ = true;
  logger.Info("[CEF] Initialized");
  return true;
}

void CefRuntime::Shutdown() {
  if (!initialized_) return;
  auto& logger = NativeLogger::Instance();
  logger.Info("[CEF] Shutdown requested");
  BrowserManager::Instance().Shutdown();
  if (const auto barrier = CreateEventW(nullptr, TRUE, FALSE, nullptr)) {
    CefPostTask(TID_UI, new SignalEventTask(barrier));
    WaitForSingleObject(barrier, 2000);
    CloseHandle(barrier);
  }
  const auto drained = LibertyCefClient::WaitForAllBrowsersClosed(std::chrono::seconds(5));
  if (!drained) logger.Warning("[CEF] Timed out while closing browsers");
  CefClearSchemeHandlerFactories();
  CefShutdown();
  initialized_ = false;
  logger.Info("[CEF] Shutdown");
}

}  // namespace liberty
