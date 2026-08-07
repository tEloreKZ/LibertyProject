#pragma once

#include "Cef/CefV8Handler.h"
#include "include/cef_app.h"

namespace liberty {

class LibertyCefApp final : public CefApp {
 public:
  LibertyCefApp();

  void OnBeforeCommandLineProcessing(const CefString& process_type,
                                     CefRefPtr<CefCommandLine> command_line) override;
  void OnRegisterCustomSchemes(CefRawPtr<CefSchemeRegistrar> registrar) override;
  CefRefPtr<CefRenderProcessHandler> GetRenderProcessHandler() override;

 private:
  CefRefPtr<LibertyRenderProcessHandler> render_handler_;
  IMPLEMENT_REFCOUNTING(LibertyCefApp);
  DISALLOW_COPY_AND_ASSIGN(LibertyCefApp);
};

}  // namespace liberty

