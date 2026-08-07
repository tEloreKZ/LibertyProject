#pragma once

#include <string>
#include <unordered_map>
#include <vector>

#include "include/cef_render_process_handler.h"
#include "include/cef_v8.h"

namespace liberty {

class LibertyV8Bridge final : public CefBaseRefCounted {
 public:
  LibertyV8Bridge(int browser_id, CefRefPtr<CefV8Context> context);

  bool Emit(const CefV8ValueList& arguments, CefString& exception);
  bool On(const CefV8ValueList& arguments, CefString& exception);
  bool Off(const CefV8ValueList& arguments, CefString& exception);
  void Dispatch(const std::string& event_name, const std::string& json);
  void Release();

 private:
  std::string Stringify(CefRefPtr<CefV8Value> value) const;
  CefRefPtr<CefV8Value> Parse(const std::string& json) const;

  int browser_id_;
  CefRefPtr<CefV8Context> context_;
  std::unordered_map<std::string, std::vector<CefRefPtr<CefV8Value>>> listeners_;
  IMPLEMENT_REFCOUNTING(LibertyV8Bridge);
  DISALLOW_COPY_AND_ASSIGN(LibertyV8Bridge);
};

class LibertyV8FunctionHandler final : public CefV8Handler {
 public:
  enum class Operation { Emit, On, Off };
  LibertyV8FunctionHandler(CefRefPtr<LibertyV8Bridge> bridge, Operation operation)
      : bridge_(std::move(bridge)), operation_(operation) {}

  bool Execute(const CefString& name, CefRefPtr<CefV8Value> object,
               const CefV8ValueList& arguments, CefRefPtr<CefV8Value>& retval,
               CefString& exception) override;

 private:
  CefRefPtr<LibertyV8Bridge> bridge_;
  Operation operation_;
  IMPLEMENT_REFCOUNTING(LibertyV8FunctionHandler);
  DISALLOW_COPY_AND_ASSIGN(LibertyV8FunctionHandler);
};

class LibertyRenderProcessHandler final : public CefRenderProcessHandler {
 public:
  LibertyRenderProcessHandler() = default;
  void OnContextCreated(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                        CefRefPtr<CefV8Context> context) override;
  void OnContextReleased(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame,
                         CefRefPtr<CefV8Context> context) override;
  bool OnProcessMessageReceived(CefRefPtr<CefBrowser> browser,
                                CefRefPtr<CefFrame> frame,
                                CefProcessId source_process,
                                CefRefPtr<CefProcessMessage> message) override;

 private:
  std::unordered_map<int, CefRefPtr<LibertyV8Bridge>> bridges_;
  IMPLEMENT_REFCOUNTING(LibertyRenderProcessHandler);
  DISALLOW_COPY_AND_ASSIGN(LibertyRenderProcessHandler);
};

}  // namespace liberty
