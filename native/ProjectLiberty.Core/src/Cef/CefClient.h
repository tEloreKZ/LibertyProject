#pragma once

#include <memory>
#include <chrono>
#include <deque>

#include "Browser.h"
#include "Cef/CefRenderHandler.h"
#include "include/cef_client.h"

namespace liberty {

class LibertyCefClient final : public CefClient,
                               public CefLifeSpanHandler,
                               public CefLoadHandler,
                               public CefRequestHandler {
 public:
  explicit LibertyCefClient(std::shared_ptr<Browser> browser);
  static bool WaitForAllBrowsersClosed(std::chrono::milliseconds timeout);

  CefRefPtr<CefRenderHandler> GetRenderHandler() override { return render_handler_; }
  CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override { return this; }
  CefRefPtr<CefLoadHandler> GetLoadHandler() override { return this; }
  CefRefPtr<CefRequestHandler> GetRequestHandler() override { return this; }
  void OnAfterCreated(CefRefPtr<CefBrowser> browser) override;
  void OnBeforeClose(CefRefPtr<CefBrowser> browser) override;
  void OnLoadEnd(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                 int http_status_code) override;
  void OnLoadError(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                   ErrorCode error_code, const CefString& error_text,
                   const CefString& failed_url) override;
  void OnRenderProcessTerminated(CefRefPtr<CefBrowser> browser,
                                 TerminationStatus status, int error_code,
                                 const CefString& error_string) override;
  bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                CefRefPtr<CefFrame> frame,
                                CefProcessId source_process,
                                CefRefPtr<CefProcessMessage> message) override;

 private:
  std::shared_ptr<Browser> browser_;
  CefRefPtr<LibertyCefRenderHandler> render_handler_;
  bool lifecycle_counted_{};
  std::deque<std::chrono::steady_clock::time_point> renderer_crashes_;
  IMPLEMENT_REFCOUNTING(LibertyCefClient);
  DISALLOW_COPY_AND_ASSIGN(LibertyCefClient);
};

}  // namespace liberty
