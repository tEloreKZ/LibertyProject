#include "ProjectLiberty/BrowserApi.h"

#include <Windows.h>

#include <cstring>
#include <string>

#include "BrowserManager.h"
#include "CoreRuntime.h"
#include "EventQueue.h"

namespace {

LibertyResult Ready() {
  return liberty::CoreRuntime::Instance().IsReady() ? LibertyResult_Ok
                                                    : LibertyResult_NotReady;
}

LibertyResult Existing(const LibertyBrowserId id) {
  if (Ready() != LibertyResult_Ok) return LibertyResult_NotReady;
  return liberty::BrowserManager::Instance().Find(id) ? LibertyResult_Ok
                                                      : LibertyResult_NotFound;
}

}  // namespace

std::uint32_t Liberty_GetApiVersion() { return 1; }

std::int32_t Liberty_IsReady() {
  return liberty::CoreRuntime::Instance().IsReady() ? 1 : 0;
}

LibertyResult Liberty_CreateBrowser(const LibertyBrowserOptions* options,
                                    LibertyBrowserId* browser_id) {
  if (Ready() != LibertyResult_Ok) return LibertyResult_NotReady;
  if (!options || !browser_id || options->struct_size < sizeof(LibertyBrowserOptions) ||
      !options->url_utf8 || !options->owner_utf8) return LibertyResult_InvalidArgument;
  liberty::BrowserOptions value;
  value.url = options->url_utf8;
  value.owner = options->owner_utf8;
  value.width = options->width;
  value.height = options->height;
  value.x = options->x;
  value.y = options->y;
  value.fps = options->fps;
  value.z_index = options->z_index;
  value.transparent = options->transparent != 0;
  value.visible = options->visible != 0;
  value.focused = options->focused != 0;
  *browser_id = liberty::BrowserManager::Instance().CreateBrowser(std::move(value));
  return *browser_id ? LibertyResult_Ok : LibertyResult_InvalidArgument;
}

LibertyResult Liberty_DestroyBrowser(const LibertyBrowserId id) {
  if (Ready() != LibertyResult_Ok) return LibertyResult_NotReady;
  return liberty::BrowserManager::Instance().DestroyBrowser(id) ? LibertyResult_Ok
                                                                 : LibertyResult_NotFound;
}

LibertyResult Liberty_DestroyBrowsersByOwner(const char* owner_utf8) {
  if (Ready() != LibertyResult_Ok) return LibertyResult_NotReady;
  if (!owner_utf8 || !*owner_utf8) return LibertyResult_InvalidArgument;
  liberty::BrowserManager::Instance().DestroyBrowsersByOwner(owner_utf8);
  return LibertyResult_Ok;
}

LibertyResult Liberty_ShowBrowser(const LibertyBrowserId id) {
  return Existing(id) != LibertyResult_Ok ? Existing(id) :
      (liberty::BrowserManager::Instance().Show(id, true) ? LibertyResult_Ok : LibertyResult_NotFound);
}
LibertyResult Liberty_HideBrowser(const LibertyBrowserId id) {
  return Existing(id) != LibertyResult_Ok ? Existing(id) :
      (liberty::BrowserManager::Instance().Show(id, false) ? LibertyResult_Ok : LibertyResult_NotFound);
}
LibertyResult Liberty_FocusBrowser(const LibertyBrowserId id) {
  return Existing(id) != LibertyResult_Ok ? Existing(id) :
      (liberty::BrowserManager::Instance().Focus(id, true) ? LibertyResult_Ok : LibertyResult_NotFound);
}
LibertyResult Liberty_UnfocusBrowser(const LibertyBrowserId id) {
  return Existing(id) != LibertyResult_Ok ? Existing(id) :
      (liberty::BrowserManager::Instance().Focus(id, false) ? LibertyResult_Ok : LibertyResult_NotFound);
}

LibertyResult Liberty_LoadBrowserUrl(const LibertyBrowserId id, const char* url_utf8) {
  if (Existing(id) != LibertyResult_Ok) return Existing(id);
  if (!url_utf8) return LibertyResult_InvalidArgument;
  return liberty::BrowserManager::Instance().LoadUrl(id, url_utf8) ? LibertyResult_Ok
                                                                   : LibertyResult_InvalidArgument;
}

LibertyResult Liberty_SetBrowserBounds(const LibertyBrowserId id, const std::int32_t x,
                                       const std::int32_t y, const std::int32_t width,
                                       const std::int32_t height) {
  if (Existing(id) != LibertyResult_Ok) return Existing(id);
  return liberty::BrowserManager::Instance().SetBounds(id, x, y, width, height)
      ? LibertyResult_Ok : LibertyResult_InvalidArgument;
}

LibertyResult Liberty_SetBrowserZIndex(const LibertyBrowserId id, const std::int32_t z_index) {
  if (Existing(id) != LibertyResult_Ok) return Existing(id);
  return liberty::BrowserManager::Instance().SetZIndex(id, z_index)
      ? LibertyResult_Ok : LibertyResult_NotFound;
}

LibertyResult Liberty_BrowserEmit(const LibertyBrowserId id, const char* event_name_utf8,
                                  const char* json_utf8) {
  if (Existing(id) != LibertyResult_Ok) return Existing(id);
  if (!event_name_utf8 || !json_utf8) return LibertyResult_InvalidArgument;
  return liberty::BrowserManager::Instance().Emit(id, event_name_utf8, json_utf8)
      ? LibertyResult_Ok : LibertyResult_InvalidArgument;
}

LibertyResult Liberty_PeekBrowserEvent(LibertyBrowserEventInfo* info) {
  if (!info || info->struct_size < sizeof(LibertyBrowserEventInfo))
    return LibertyResult_InvalidArgument;
  const auto event = liberty::EventQueue::Instance().Peek();
  if (!event) return LibertyResult_NotFound;
  info->browser_id = event->browser_id;
  info->event_name_bytes = static_cast<std::uint32_t>(event->name.size() + 1);
  info->json_bytes = static_cast<std::uint32_t>(event->json.size() + 1);
  return LibertyResult_Ok;
}

LibertyResult Liberty_PollBrowserEvent(char* event_name_utf8,
                                       const std::uint32_t event_name_capacity,
                                       char* json_utf8, const std::uint32_t json_capacity,
                                       LibertyBrowserEventInfo* info) {
  const auto peek_result = Liberty_PeekBrowserEvent(info);
  if (peek_result != LibertyResult_Ok) return peek_result;
  if (!event_name_utf8 || !json_utf8 || event_name_capacity < info->event_name_bytes ||
      json_capacity < info->json_bytes) return LibertyResult_BufferTooSmall;
  const auto event = liberty::EventQueue::Instance().Pop();
  if (!event) return LibertyResult_NotFound;
  info->browser_id = event->browser_id;
  strcpy_s(event_name_utf8, event_name_capacity, event->name.c_str());
  strcpy_s(json_utf8, json_capacity, event->json.c_str());
  return LibertyResult_Ok;
}

BOOL APIENTRY DllMain(HMODULE module, const DWORD reason, LPVOID) {
  if (reason == DLL_PROCESS_ATTACH) {
    DisableThreadLibraryCalls(module);
    return liberty::CoreRuntime::Instance().Start(module) ? TRUE : FALSE;
  }
  if (reason == DLL_PROCESS_DETACH) liberty::CoreRuntime::Instance().RequestStop();
  return TRUE;
}
