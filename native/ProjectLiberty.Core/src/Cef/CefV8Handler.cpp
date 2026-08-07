#include "Cef/CefV8Handler.h"

#include <algorithm>

#include "include/cef_process_message.h"

namespace liberty {

LibertyV8Bridge::LibertyV8Bridge(const int browser_id, CefRefPtr<CefV8Context> context)
    : browser_id_(browser_id), context_(std::move(context)) {}

bool LibertyV8Bridge::Emit(const CefV8ValueList& arguments, CefString& exception) {
  if (arguments.empty() || !arguments[0]->IsString()) {
    exception = "liberty.emit requires an event name";
    return true;
  }
  const std::string event_name = arguments[0]->GetStringValue();
  if (event_name.empty() || event_name.size() > 128) {
    exception = "Invalid event name";
    return true;
  }
  std::string json = "null";
  if (arguments.size() > 1 && !arguments[1]->IsUndefined()) {
    json = Stringify(arguments[1]);
    if (json.empty()) {
      exception = "Event data is not JSON serializable";
      return true;
    }
  }
  auto message = CefProcessMessage::Create("liberty.emit");
  message->GetArgumentList()->SetString(0, event_name);
  message->GetArgumentList()->SetString(1, json);
  if (context_ && context_->GetFrame())
    context_->GetFrame()->SendProcessMessage(PID_BROWSER, message);
  return true;
}

bool LibertyV8Bridge::On(const CefV8ValueList& arguments, CefString& exception) {
  if (arguments.size() < 2 || !arguments[0]->IsString() || !arguments[1]->IsFunction()) {
    exception = "liberty.on requires an event name and callback";
    return true;
  }
  const std::string event_name = arguments[0]->GetStringValue();
  auto& listeners = listeners_[event_name];
  if (std::ranges::none_of(listeners, [&](const CefRefPtr<CefV8Value>& item) {
        return item->IsSame(arguments[1]);
      })) listeners.push_back(arguments[1]);
  return true;
}

bool LibertyV8Bridge::Off(const CefV8ValueList& arguments, CefString& exception) {
  if (arguments.empty() || !arguments[0]->IsString()) {
    exception = "liberty.off requires an event name";
    return true;
  }
  const std::string event_name = arguments[0]->GetStringValue();
  auto iterator = listeners_.find(event_name);
  if (iterator == listeners_.end()) return true;
  if (arguments.size() < 2 || !arguments[1]->IsFunction()) {
    listeners_.erase(iterator);
    return true;
  }
  std::erase_if(iterator->second, [&](const CefRefPtr<CefV8Value>& item) {
    return item->IsSame(arguments[1]);
  });
  if (iterator->second.empty()) listeners_.erase(iterator);
  return true;
}

void LibertyV8Bridge::Dispatch(const std::string& event_name, const std::string& json) {
  const auto iterator = listeners_.find(event_name);
  if (iterator == listeners_.end() || !context_ || !context_->Enter()) return;
  const auto payload = Parse(json);
  if (payload) {
    const CefV8ValueList arguments{payload};
    const auto listeners = iterator->second;
    for (const auto& listener : listeners) listener->ExecuteFunction(nullptr, arguments);
  }
  context_->Exit();
}

void LibertyV8Bridge::Release() {
  listeners_.clear();
  context_ = nullptr;
}

std::string LibertyV8Bridge::Stringify(CefRefPtr<CefV8Value> value) const {
  if (!context_) return {};
  const auto json = context_->GetGlobal()->GetValue("JSON");
  const auto stringify = json ? json->GetValue("stringify") : nullptr;
  if (!stringify || !stringify->IsFunction()) return {};
  const CefV8ValueList arguments{value};
  const auto result = stringify->ExecuteFunction(json, arguments);
  return result && result->IsString() ? result->GetStringValue().ToString() : std::string{};
}

CefRefPtr<CefV8Value> LibertyV8Bridge::Parse(const std::string& value) const {
  if (!context_) return nullptr;
  const auto json = context_->GetGlobal()->GetValue("JSON");
  const auto parse = json ? json->GetValue("parse") : nullptr;
  if (!parse || !parse->IsFunction()) return nullptr;
  const CefV8ValueList arguments{CefV8Value::CreateString(value)};
  return parse->ExecuteFunction(json, arguments);
}

bool LibertyV8FunctionHandler::Execute(const CefString&, CefRefPtr<CefV8Value>,
                                       const CefV8ValueList& arguments,
                                       CefRefPtr<CefV8Value>& retval,
                                       CefString& exception) {
  retval = CefV8Value::CreateUndefined();
  switch (operation_) {
    case Operation::Emit: return bridge_->Emit(arguments, exception);
    case Operation::On: return bridge_->On(arguments, exception);
    case Operation::Off: return bridge_->Off(arguments, exception);
  }
  return false;
}

void LibertyRenderProcessHandler::OnContextCreated(CefRefPtr<CefBrowser> browser,
                                                   CefRefPtr<CefFrame> frame,
                                                   CefRefPtr<CefV8Context> context) {
  if (!frame->IsMain()) return;
  const auto browser_id = browser->GetIdentifier();
  auto bridge = new LibertyV8Bridge(browser_id, context);
  auto object = CefV8Value::CreateObject(nullptr, nullptr);
  object->SetValue("emit", CefV8Value::CreateFunction("emit",
      new LibertyV8FunctionHandler(bridge, LibertyV8FunctionHandler::Operation::Emit)),
      V8_PROPERTY_ATTRIBUTE_READONLY);
  object->SetValue("on", CefV8Value::CreateFunction("on",
      new LibertyV8FunctionHandler(bridge, LibertyV8FunctionHandler::Operation::On)),
      V8_PROPERTY_ATTRIBUTE_READONLY);
  object->SetValue("off", CefV8Value::CreateFunction("off",
      new LibertyV8FunctionHandler(bridge, LibertyV8FunctionHandler::Operation::Off)),
      V8_PROPERTY_ATTRIBUTE_READONLY);
  context->GetGlobal()->SetValue("liberty", object,
      static_cast<CefV8Value::PropertyAttribute>(V8_PROPERTY_ATTRIBUTE_READONLY |
                                                V8_PROPERTY_ATTRIBUTE_DONTDELETE));
  bridges_[browser_id] = bridge;
}

void LibertyRenderProcessHandler::OnContextReleased(CefRefPtr<CefBrowser> browser,
                                                    CefRefPtr<CefFrame> frame,
                                                    CefRefPtr<CefV8Context>) {
  if (!frame->IsMain()) return;
  const auto iterator = bridges_.find(browser->GetIdentifier());
  if (iterator == bridges_.end()) return;
  iterator->second->Release();
  bridges_.erase(iterator);
}

bool LibertyRenderProcessHandler::OnProcessMessageReceived(
    CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame>, const CefProcessId source_process,
    CefRefPtr<CefProcessMessage> message) {
  if (source_process != PID_BROWSER || message->GetName() != "liberty.event") return false;
  const auto iterator = bridges_.find(browser->GetIdentifier());
  if (iterator == bridges_.end()) return true;
  const auto arguments = message->GetArgumentList();
  if (arguments->GetSize() >= 2)
    iterator->second->Dispatch(arguments->GetString(0), arguments->GetString(1));
  return true;
}

}  // namespace liberty

