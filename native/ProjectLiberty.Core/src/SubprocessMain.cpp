#include <Windows.h>

#include "Cef/CefApp.h"
#include "include/cef_app.h"
#include "include/cef_sandbox_win.h"
#include "include/cef_version_info.h"

CEF_BOOTSTRAP_EXPORT int RunWinMain(HINSTANCE instance, LPTSTR, int,
                                    void* sandbox_info,
                                    cef_version_info_t*) {
  CefMainArgs arguments(instance);
  return CefExecuteProcess(arguments, new liberty::LibertyCefApp(), sandbox_info);
}
