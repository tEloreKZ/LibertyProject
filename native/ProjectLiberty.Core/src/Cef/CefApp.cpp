#include "Cef/CefApp.h"

namespace liberty {

LibertyCefApp::LibertyCefApp() : render_handler_(new LibertyRenderProcessHandler()) {}

void LibertyCefApp::OnBeforeCommandLineProcessing(
    const CefString&, CefRefPtr<CefCommandLine> command_line) {
  command_line->AppendSwitch("no-sandbox");
  command_line->AppendSwitch("disable-background-networking");
  command_line->AppendSwitch("disable-breakpad");
  command_line->AppendSwitch("disable-component-update");
  command_line->AppendSwitch("disable-extensions");
  command_line->AppendSwitch("disable-sync");
  command_line->AppendSwitch("mute-audio");
}

void LibertyCefApp::OnRegisterCustomSchemes(CefRawPtr<CefSchemeRegistrar> registrar) {
  registrar->AddCustomScheme("liberty", CEF_SCHEME_OPTION_STANDARD |
      CEF_SCHEME_OPTION_SECURE | CEF_SCHEME_OPTION_CORS_ENABLED |
      CEF_SCHEME_OPTION_FETCH_ENABLED);
}

CefRefPtr<CefRenderProcessHandler> LibertyCefApp::GetRenderProcessHandler() {
  return render_handler_;
}

}  // namespace liberty

