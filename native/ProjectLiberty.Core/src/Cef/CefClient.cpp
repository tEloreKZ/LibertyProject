#include "Cef/CefClient.h"

#include "BrowserManager.h"
#include "Logging/NativeLogger.h"

#include <condition_variable>

namespace {
std::mutex lifecycle_mutex;
std::condition_variable lifecycle_condition;
int active_browsers{};
}

namespace liberty {

LibertyCefClient::LibertyCefClient(std::shared_ptr<Browser> browser)
    : browser_(std::move(browser)),
      render_handler_(new LibertyCefRenderHandler(browser_)) {}

void LibertyCefClient::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
  {
    std::scoped_lock lock(lifecycle_mutex);
    ++active_browsers;
    lifecycle_counted_ = true;
  }
  BrowserManager::Instance().AttachCefBrowser(browser_->id(), browser);
}

void LibertyCefClient::OnBeforeClose(CefRefPtr<CefBrowser>) {
  BrowserManager::Instance().OnBrowserClosed(browser_->id());
  browser_->SetCefBrowser(nullptr);
  if (lifecycle_counted_) {
    {
      std::scoped_lock lock(lifecycle_mutex);
      --active_browsers;
      lifecycle_counted_ = false;
    }
    lifecycle_condition.notify_all();
  }
}

bool LibertyCefClient::WaitForAllBrowsersClosed(const std::chrono::milliseconds timeout) {
  std::unique_lock lock(lifecycle_mutex);
  return lifecycle_condition.wait_for(lock, timeout, [] { return active_browsers == 0; });
}

void LibertyCefClient::OnLoadEnd(CefRefPtr<CefBrowser>, CefRefPtr<CefFrame> frame,
                                 const int http_status_code) {
  if (!frame->IsMain()) return;
  renderer_crashes_.clear();
  NativeLogger::Instance().Info("[CEF] Browser #" + std::to_string(browser_->id()) +
                                " loaded (HTTP " + std::to_string(http_status_code) + ")");
}

void LibertyCefClient::OnLoadError(CefRefPtr<CefBrowser>, CefRefPtr<CefFrame> frame,
                                   const ErrorCode error_code,
                                   const CefString& error_text,
                                   const CefString& failed_url) {
  if (!frame->IsMain() || error_code == ERR_ABORTED) return;
  NativeLogger::Instance().Error("[CEF] Browser #" + std::to_string(browser_->id()) +
      " failed URL " + failed_url.ToString() + ": " + error_text.ToString());
}

void LibertyCefClient::OnRenderProcessTerminated(CefRefPtr<CefBrowser> browser,
                                                 const TerminationStatus status,
                                                 const int error_code,
                                                 const CefString& error_string) {
  NativeLogger::Instance().Error("[CEF] Renderer for browser #" +
      std::to_string(browser_->id()) + " crashed (status " + std::to_string(status) +
      ", code " + std::to_string(error_code) + "): " + error_string.ToString());
  const auto now = std::chrono::steady_clock::now();
  renderer_crashes_.push_back(now);
  while (!renderer_crashes_.empty() &&
         now - renderer_crashes_.front() > std::chrono::seconds(30)) {
    renderer_crashes_.pop_front();
  }
  if (renderer_crashes_.size() <= 3) {
    NativeLogger::Instance().Warning("[CEF] Recovering browser #" +
                                     std::to_string(browser_->id()));
    browser->Reload();
    return;
  }
  NativeLogger::Instance().Error("[CEF] Browser #" + std::to_string(browser_->id()) +
      " disabled after repeated renderer crashes; automatic reload stopped");
}

bool LibertyCefClient::OnProcessMessageReceived(CefRefPtr<CefBrowser>, CefRefPtr<CefFrame>,
                                                const CefProcessId source_process,
                                                CefRefPtr<CefProcessMessage> message) {
  if (source_process != PID_RENDERER || message->GetName() != "liberty.emit") return false;
  const auto arguments = message->GetArgumentList();
  if (arguments->GetSize() < 2) return true;
  BrowserManager::Instance().OnRendererEvent(
      browser_->id(), arguments->GetString(0), arguments->GetString(1));
  return true;
}

}  // namespace liberty
